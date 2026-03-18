# Script para configurar GitHub Actions para PhotosMarket
# Este script crea el Service Principal y muestra los secrets necesarios

Write-Host "`n╔════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║   CONFIGURACIÓN DE GITHUB ACTIONS - PHOTOSMARKET   ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════╝`n" -ForegroundColor Cyan

# Verificar que Azure CLI esté instalado
try {
    az version | Out-Null
    Write-Host "✓ Azure CLI detectado" -ForegroundColor Green
} catch {
    Write-Host "✗ Azure CLI no está instalado" -ForegroundColor Red
    Write-Host "Instálalo desde: https://docs.microsoft.com/cli/azure/install-azure-cli" -ForegroundColor Yellow
    exit 1
}

# Verificar login
Write-Host "`nVerificando sesión de Azure..." -ForegroundColor Yellow
$account = az account show 2>$null | ConvertFrom-Json

if (-not $account) {
    Write-Host "No has iniciado sesión en Azure. Iniciando login..." -ForegroundColor Yellow
    az login
    $account = az account show | ConvertFrom-Json
}

Write-Host "✓ Sesión activa" -ForegroundColor Green
Write-Host "  Suscripción: $($account.name)" -ForegroundColor White
Write-Host "  ID: $($account.id)" -ForegroundColor White

# Obtener información actual
$subscriptionId = $account.id
$resourceGroup = "rg-photosmarket-dev"

Write-Host "`n════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "PASO 1: CREAR SERVICE PRINCIPAL" -ForegroundColor Yellow
Write-Host "════════════════════════════════════════════════════════`n" -ForegroundColor Cyan

Write-Host "Creando Service Principal 'photosmarket-github-actions'..." -ForegroundColor White

# Crear Service Principal
$spName = "photosmarket-github-actions-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
$scope = "/subscriptions/$subscriptionId/resourceGroups/$resourceGroup"

try {
    $spOutput = az ad sp create-for-rbac `
        --name $spName `
        --role contributor `
        --scopes $scope `
        --sdk-auth 2>&1

    if ($LASTEXITCODE -ne 0) {
        Write-Host "✗ Error creando Service Principal" -ForegroundColor Red
        Write-Host $spOutput -ForegroundColor Red
        exit 1
    }

    $spJson = $spOutput | ConvertFrom-Json
    Write-Host "✓ Service Principal creado exitosamente" -ForegroundColor Green

} catch {
    Write-Host "✗ Error: $_" -ForegroundColor Red
    exit 1
}

# Asignar permisos de ACR
Write-Host "`n════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "PASO 2: ASIGNAR PERMISOS DE CONTAINER REGISTRY" -ForegroundColor Yellow
Write-Host "════════════════════════════════════════════════════════`n" -ForegroundColor Cyan

$acrName = "photosmarketacrdev"
Write-Host "Obteniendo ID del Container Registry..." -ForegroundColor White

try {
    $acrId = az acr show --name $acrName --query id -o tsv 2>$null
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Asignando rol AcrPull al Service Principal..." -ForegroundColor White
        az role assignment create `
            --assignee $spJson.clientId `
            --role AcrPull `
            --scope $acrId | Out-Null

        if ($LASTEXITCODE -eq 0) {
            Write-Host "✓ Permisos de ACR asignados" -ForegroundColor Green
        } else {
            Write-Host "⚠ Error asignando permisos de ACR (puede que ya existan)" -ForegroundColor Yellow
        }
    } else {
        Write-Host "⚠ Container Registry no encontrado (puede configurarse después)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "⚠ Error configurando ACR: $_" -ForegroundColor Yellow
}

# Generar JWT Secret
Write-Host "`n════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "PASO 3: GENERAR JWT SECRET KEY" -ForegroundColor Yellow
Write-Host "════════════════════════════════════════════════════════`n" -ForegroundColor Cyan

$jwtSecret = -join ((48..57) + (65..90) + (97..122) | Get-Random -Count 64 | ForEach-Object {[char]$_})
Write-Host "✓ JWT Secret Key generada (64 caracteres)" -ForegroundColor Green

# Mostrar resumen
Write-Host "`n╔════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║              CONFIGURACIÓN COMPLETADA              ║" -ForegroundColor Green
Write-Host "╚════════════════════════════════════════════════════════╝`n" -ForegroundColor Green

Write-Host "AHORA DEBES CONFIGURAR ESTOS SECRETS EN GITHUB:" -ForegroundColor Cyan
Write-Host "================================================`n" -ForegroundColor Cyan

Write-Host "1. Ve a tu repositorio en GitHub" -ForegroundColor White
Write-Host "2. Settings → Secrets and variables → Actions → New repository secret" -ForegroundColor White
Write-Host "3. Crea los siguientes 5 secrets:`n" -ForegroundColor White

# Guardar en archivo temporal
$secretsFile = "$PSScriptRoot\github-secrets-temp.txt"
$secretsContent = @"
════════════════════════════════════════════════════════
GITHUB SECRETS - PHOTOSMARKET
Generado: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
════════════════════════════════════════════════════════

═══ SECRET 1: AZURE_CREDENTIALS ═══
$($spOutput)

═══ SECRET 2: GOOGLE_OAUTH_CLIENT_ID ═══
[OBTENER DE GOOGLE CLOUD CONSOLE]
https://console.cloud.google.com/apis/credentials

═══ SECRET 3: GOOGLE_OAUTH_CLIENT_SECRET ═══
[OBTENER DE GOOGLE CLOUD CONSOLE]
https://console.cloud.google.com/apis/credentials

═══ SECRET 4: JWT_SECRET_KEY ═══
$jwtSecret

═══ SECRET 5: GOOGLE_DRIVE_ROOT_FOLDER_ID ═══
[OBTENER DE LA URL DE TU CARPETA EN GOOGLE DRIVE]
Ejemplo: Si la URL es https://drive.google.com/drive/folders/ABC123XYZ
El ID sería: ABC123XYZ

════════════════════════════════════════════════════════
INFORMACIÓN ADICIONAL
════════════════════════════════════════════════════════

Service Principal Name: $spName
Client ID: $($spJson.clientId)
Tenant ID: $($spJson.tenantId)
Subscription ID: $subscriptionId
Resource Group: $resourceGroup
Container Registry: $acrName

════════════════════════════════════════════════════════
PRÓXIMOS PASOS
════════════════════════════════════════════════════════

1. Copiar cada valor a GitHub Secrets (uno por uno)
2. Obtener Client ID y Secret de Google Cloud Console
3. Obtener Google Drive Root Folder ID
4. Commit y push de los workflows:
   git add .github/
   git commit -m "ci: add GitHub Actions workflows"
   git push origin main

5. Ver el primer despliegue en GitHub Actions

════════════════════════════════════════════════════════
"@

$secretsContent | Out-File -FilePath $secretsFile -Encoding UTF8

Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Yellow
Write-Host "SECRET 1 - AZURE_CREDENTIALS" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Yellow
Write-Host "Nombre: AZURE_CREDENTIALS" -ForegroundColor White
Write-Host "Valor: (Copiado al portapapeles)"`n -ForegroundColor Green

# Copiar al portapapeles
$spOutput | Set-Clipboard
Write-Host "✓ JSON del Service Principal copiado al portapapeles" -ForegroundColor Green
Write-Host "  → Pégalo directamente en GitHub como valor de AZURE_CREDENTIALS"`n -ForegroundColor White

Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Yellow
Write-Host "SECRET 2 - GOOGLE_OAUTH_CLIENT_ID" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Yellow
Write-Host "Nombre: GOOGLE_OAUTH_CLIENT_ID" -ForegroundColor White
Write-Host "Valor: [OBTENER DE GOOGLE CLOUD CONSOLE]" -ForegroundColor Yellow
Write-Host "  → https://console.cloud.google.com/apis/credentials"`n -ForegroundColor White

Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Yellow
Write-Host "SECRET 3 - GOOGLE_OAUTH_CLIENT_SECRET" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Yellow
Write-Host "Nombre: GOOGLE_OAUTH_CLIENT_SECRET" -ForegroundColor White
Write-Host "Valor: [OBTENER DE GOOGLE CLOUD CONSOLE]" -ForegroundColor Yellow
Write-Host "  → https://console.cloud.google.com/apis/credentials"`n -ForegroundColor White

Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Yellow
Write-Host "SECRET 4 - JWT_SECRET_KEY" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Yellow
Write-Host "Nombre: JWT_SECRET_KEY" -ForegroundColor White
Write-Host "Valor:" -ForegroundColor White
Write-Host $jwtSecret -ForegroundColor Green
Write-Host ""

Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Yellow
Write-Host "SECRET 5 - GOOGLE_DRIVE_ROOT_FOLDER_ID" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Yellow
Write-Host "Nombre: GOOGLE_DRIVE_ROOT_FOLDER_ID" -ForegroundColor White
Write-Host "Valor: [ID DE TU CARPETA DE GOOGLE DRIVE]" -ForegroundColor Yellow
Write-Host "  → Abre la carpeta en Google Drive y copia el ID de la URL"`n -ForegroundColor White

Write-Host "`n════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "📄 INFORMACIÓN GUARDADA" -ForegroundColor Yellow
Write-Host "════════════════════════════════════════════════════════`n" -ForegroundColor Cyan

Write-Host "Todos los valores han sido guardados en:" -ForegroundColor White
Write-Host "  $secretsFile" -ForegroundColor Cyan
Write-Host "`n⚠ IMPORTANTE: Este archivo contiene información sensible." -ForegroundColor Yellow
Write-Host "   Elimínalo después de configurar GitHub Secrets.`n" -ForegroundColor Yellow

Write-Host "════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "🚀 PRÓXIMOS PASOS" -ForegroundColor Yellow
Write-Host "════════════════════════════════════════════════════════`n" -ForegroundColor Cyan

Write-Host "1. Configurar los 5 secrets en GitHub (usa los valores de arriba)" -ForegroundColor White
Write-Host "2. Hacer commit de los workflows:" -ForegroundColor White
Write-Host "   git add .github/" -ForegroundColor Cyan
Write-Host "   git commit -m `"ci: add GitHub Actions workflows`"" -ForegroundColor Cyan
Write-Host "   git push origin main" -ForegroundColor Cyan
Write-Host "`n3. Ver el despliegue en: https://github.com/TU_USUARIO/TU_REPO/actions" -ForegroundColor White
Write-Host ""

Write-Host "════════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host "✅ SCRIPT COMPLETADO" -ForegroundColor Green
Write-Host "════════════════════════════════════════════════════════`n" -ForegroundColor Green

# Preguntar si abrir GitHub
$openGitHub = Read-Host "¿Abrir GitHub para configurar secrets ahora? (S/N)"
if ($openGitHub -eq 'S' -or $openGitHub -eq 's') {
    Write-Host "`nAbriendo GitHub..." -ForegroundColor Cyan
    Start-Process "https://github.com/settings/tokens"
    Write-Host "⚠ Nota: Ajusta la URL con tu repositorio específico" -ForegroundColor Yellow
}

Write-Host "`n¡Listo! Sigue los próximos pasos para completar la configuración.`n" -ForegroundColor Green
