# Azure Deployment Script for PhotosMarket
# Este script despliega la infraestructura SIN SECRETOS
# Los secretos deben configurarse manualmente después del despliegue

param(
    [Parameter(Mandatory=$false)]
    [string]$ResourceGroupName = "rg-photosmarket-dev",
    
    [Parameter(Mandatory=$false)]
    [string]$Location = "eastus",
    
    [Parameter(Mandatory=$false)]
    [string]$Environment = "dev",

    [Parameter(Mandatory=$false)]
    [switch]$SkipBuild,

    [Parameter(Mandatory=$false)]
    [string]$GoogleDriveFolderId = ""
)

$ErrorActionPreference = "Stop"

Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  PHOTOSMARKET - AZURE DEPLOYMENT SCRIPT" -ForegroundColor Cyan
Write-Host "  INFRAESTRUCTURA BASE (SIN SECRETOS)" -ForegroundColor Yellow
Write-Host "═══════════════════════════════════════════════════════════`n" -ForegroundColor Cyan

# Check if Azure CLI is installed
Write-Host "→ Verificando Azure CLI..." -ForegroundColor Yellow
if (-not (Get-Command az -ErrorAction SilentlyContinue)) {
    Write-Host "✗ Azure CLI no está instalado." -ForegroundColor Red
    Write-Host "  Instala Azure CLI desde: https://docs.microsoft.com/cli/azure/install-azure-cli" -ForegroundColor White
    exit 1
}
Write-Host "✓ Azure CLI encontrado`n" -ForegroundColor Green

# Login to Azure
Write-Host "→ Verificando sesión de Azure..." -ForegroundColor Yellow
$account = az account show 2>$null | ConvertFrom-Json
if (-not $account) {
    Write-Host "  Iniciando sesión en Azure..." -ForegroundColor White
    az login
    if ($LASTEXITCODE -ne 0) {
        Write-Host "✗ Error al iniciar sesión en Azure" -ForegroundColor Red
        exit 1
    }
}
Write-Host "✓ Sesión activa: $($account.name)`n" -ForegroundColor Green

# Create Resource Group
Write-Host "→ Creando/Verificando Resource Group..." -ForegroundColor Yellow
az group create --name $ResourceGroupName --location $Location --output none
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Error al crear Resource Group" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Resource Group: $ResourceGroupName`n" -ForegroundColor Green

# Set variables
$appName = "photosmarket"
$acrName = "${appName}acr${Environment}"
$backendImageTag = "${acrName}.azurecr.io/photosmarket-backend:latest"
$frontendImageTag = "${acrName}.azurecr.io/photosmarket-frontend:latest"

# Build and push Docker images
if (-not $SkipBuild) {
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "  CONSTRUYENDO IMÁGENES DOCKER" -ForegroundColor Cyan
    Write-Host "═══════════════════════════════════════════════════════════`n" -ForegroundColor Cyan

    # Deploy infrastructure first to create ACR
    Write-Host "→ Desplegando infraestructura inicial (ACR)..." -ForegroundColor Yellow
    az deployment group create `
        --resource-group $ResourceGroupName `
        --template-file "../infra/main.bicep" `
        --parameters "../infra/main.bicepparam" `
        --parameters environmentName=$Environment `
        --output none
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "✗ Error al desplegar infraestructura" -ForegroundColor Red
        exit 1
    }
    Write-Host "✓ Infraestructura desplegada`n" -ForegroundColor Green

    # Login to ACR
    Write-Host "→ Iniciando sesión en Azure Container Registry..." -ForegroundColor Yellow
    az acr login --name $acrName
    if ($LASTEXITCODE -ne 0) {
        Write-Host "✗ Error al iniciar sesión en ACR" -ForegroundColor Red
        exit 1
    }
    Write-Host "✓ Sesión ACR iniciada`n" -ForegroundColor Green

    # Build and push backend
    Write-Host "→ Construyendo imagen del backend..." -ForegroundColor Yellow
    docker build -t $backendImageTag -f ../src/backend/Dockerfile ../src/backend
    if ($LASTEXITCODE -ne 0) {
        Write-Host "✗ Error al construir imagen del backend" -ForegroundColor Red
        exit 1
    }
    Write-Host "✓ Imagen del backend construida`n" -ForegroundColor Green

    Write-Host "→ Publicando imagen del backend..." -ForegroundColor Yellow
    docker push $backendImageTag
    if ($LASTEXITCODE -ne 0) {
        Write-Host "✗ Error al publicar imagen del backend" -ForegroundColor Red
        exit 1
    }
    Write-Host "✓ Imagen del backend publicada`n" -ForegroundColor Green

    # Build and push frontend
    Write-Host "→ Construyendo imagen del frontend..." -ForegroundColor Yellow
    docker build -t $frontendImageTag -f ../src/frontend/Dockerfile ../src/frontend
    if ($LASTEXITCODE -ne 0) {
        Write-Host "✗ Error al construir imagen del frontend" -ForegroundColor Red
        exit 1
    }
    Write-Host "✓ Imagen del frontend construida`n" -ForegroundColor Green

    Write-Host "→ Publicando imagen del frontend..." -ForegroundColor Yellow
    docker push $frontendImageTag
    if ($LASTEXITCODE -ne 0) {
        Write-Host "✗ Error al publicar imagen del frontend" -ForegroundColor Red
        exit 1
    }
    Write-Host "✓ Imagen del frontend publicada`n" -ForegroundColor Green
}

# Deploy infrastructure
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  DESPLEGANDO INFRAESTRUCTURA BASE" -ForegroundColor Cyan
Write-Host "  (Container Registry, Container Apps, Cosmos DB, Key Vault)" -ForegroundColor Yellow
Write-Host "═══════════════════════════════════════════════════════════`n" -ForegroundColor Cyan

Write-Host "→ Desplegando recursos de Azure (SIN SECRETOS)..." -ForegroundColor Yellow

# Parámetros mínimos para el despliegue inicial
$deployParams = @{
    environmentName = $Environment
}

# Solo agregar backendImage y frontendImage si NO se saltó el build
if (-not $SkipBuild) {
    $deployParams.backendImage = $backendImageTag
    $deployParams.frontendImage = $frontendImageTag
}

# Agregar Google Drive Folder ID si se proporcionó
if ($GoogleDriveFolderId) {
    $deployParams.googleDriveRootFolderId = $GoogleDriveFolderId
} else {
    $deployParams.googleDriveRootFolderId = "PLACEHOLDER"
}

# Secretos con valores placeholder (deben configurarse manualmente después)
$deployParams.googleOAuthClientId = "PLACEHOLDER"
$deployParams.googleOAuthClientSecret = "PLACEHOLDER"
$deployParams.jwtSecretKey = "PLACEHOLDER"

$parametersJson = $deployParams | ConvertTo-Json -Compress
$parametersJson | Out-File -FilePath "$PSScriptRoot\temp-params.json" -Encoding utf8

$deployment = az deployment group create `
    --resource-group $ResourceGroupName `
    --template-file "$PSScriptRoot\..\infra\main.bicep" `
    --parameters "@$PSScriptRoot\temp-params.json" `
    --output json | ConvertFrom-Json

# Limpiar archivo temporal
Remove-Item "$PSScriptRoot\temp-params.json" -ErrorAction SilentlyContinue

if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Error al desplegar infraestructura" -ForegroundColor Red
    exit 1
}

Write-Host "✓ Infraestructura base desplegada exitosamente`n" -ForegroundColor Green

# Wait for Container Apps to be ready
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  ESPERANDO PROPAGACIÓN DE RECURSOS" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════`n" -ForegroundColor Cyan

Write-Host "→ Esperando que los Container Apps estén listos..." -ForegroundColor Yellow
Start-Sleep -Seconds 30
Write-Host "✓ Recursos propagados`n" -ForegroundColor Green

# Display outputs
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host "  ✓ DESPLIEGUE DE INFRAESTRUCTURA COMPLETADO" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════════`n" -ForegroundColor Green

Write-Host "URLS DE LA APLICACIÓN:" -ForegroundColor Cyan
Write-Host "  Frontend: $($deployment.properties.outputs.frontendUrl.value)" -ForegroundColor White
Write-Host "  Backend:  $($deployment.properties.outputs.backendUrl.value)" -ForegroundColor White
Write-Host ""

Write-Host "RECURSOS DESPLEGADOS:" -ForegroundColor Cyan
Write-Host "  Container Registry: $($deployment.properties.outputs.containerRegistryLoginServer.value)" -ForegroundColor White
Write-Host "  CosmosDB:           $($deployment.properties.outputs.cosmosDbEndpoint.value)" -ForegroundColor White
$keyVaultName = $deployment.properties.outputs.keyVaultName.value
Write-Host "  Key Vault:          $keyVaultName" -ForegroundColor White
Write-Host ""

# Guardar nombres de recursos para referencia
$backendAppName = "photosmarket-backend-$Environment"
$frontendAppName = "photosmarket-frontend-$Environment"

Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Yellow
Write-Host "  ⚠ CONFIGURACIONES MANUALES REQUERIDAS" -ForegroundColor Yellow
Write-Host "═══════════════════════════════════════════════════════════`n" -ForegroundColor Yellow

Write-Host "La infraestructura está desplegada pero necesitas configurar los siguientes secretos:" -ForegroundColor White
Write-Host ""

Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Gray
Write-Host "1. GOOGLE OAUTH CLIENT ID Y SECRET" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Gray
Write-Host ""
Write-Host "Paso 1: Obtén las credenciales de Google Cloud Console" -ForegroundColor Yellow
Write-Host "  → https://console.cloud.google.com/apis/credentials" -ForegroundColor White
Write-Host ""
Write-Host "Paso 2: Configura los secretos en Key Vault:" -ForegroundColor Yellow
Write-Host ""
Write-Host "  # Google OAuth Client ID" -ForegroundColor Green
Write-Host "  az keyvault secret set ``" -ForegroundColor White
Write-Host "    --vault-name $keyVaultName ``" -ForegroundColor White
Write-Host "    --name 'GoogleOAuthClientId' ``" -ForegroundColor White
Write-Host "    --value 'TU_CLIENT_ID_AQUI'" -ForegroundColor White
Write-Host ""
Write-Host "  # Google OAuth Client Secret" -ForegroundColor Green
Write-Host "  az keyvault secret set ``" -ForegroundColor White
Write-Host "    --vault-name $keyVaultName ``" -ForegroundColor White
Write-Host "    --name 'GoogleOAuthClientSecret' ``" -ForegroundColor White
Write-Host "    --value 'TU_CLIENT_SECRET_AQUI'" -ForegroundColor White
Write-Host ""

Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Gray
Write-Host "2. JWT SECRET KEY" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Gray
Write-Host ""
Write-Host "Genera una clave segura de al menos 32 caracteres:" -ForegroundColor Yellow
Write-Host ""
Write-Host "  # PowerShell - Generar clave aleatoria" -ForegroundColor Green
Write-Host "  `$jwtKey = -join ((48..57) + (65..90) + (97..122) | Get-Random -Count 64 | % {[char]`$_})" -ForegroundColor White
Write-Host "  Write-Host `$jwtKey" -ForegroundColor White
Write-Host ""
Write-Host "  # Luego guardarla en Key Vault" -ForegroundColor Green
Write-Host "  az keyvault secret set ``" -ForegroundColor White
Write-Host "    --vault-name $keyVaultName ``" -ForegroundColor White
Write-Host "    --name 'JwtSecretKey' ``" -ForegroundColor White
Write-Host "    --value `"`$jwtKey`"" -ForegroundColor White
Write-Host ""

Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Gray
Write-Host "3. GOOGLE DRIVE FOLDER ID (Opcional)" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Gray
Write-Host ""
Write-Host "Si quieres cambiar el Folder ID de Google Drive:" -ForegroundColor Yellow
Write-Host ""
Write-Host "  az keyvault secret set ``" -ForegroundColor White
Write-Host "    --vault-name $keyVaultName ``" -ForegroundColor White
Write-Host "    --name 'GoogleDriveFolderId' ``" -ForegroundColor White
Write-Host "    --value 'TU_FOLDER_ID_AQUI'" -ForegroundColor White
Write-Host ""

Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Gray
Write-Host "4. CREDENCIALES DE GOOGLE DRIVE (Service Account)" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Gray
Write-Host ""
Write-Host "Si usas Google Drive API con Service Account:" -ForegroundColor Yellow
Write-Host ""
Write-Host "  # Sube el archivo de credenciales como secreto" -ForegroundColor Green
Write-Host "  `$credentialsJson = Get-Content 'google-drive-credentials.json' -Raw" -ForegroundColor White
Write-Host "  az keyvault secret set ``" -ForegroundColor White
Write-Host "    --vault-name $keyVaultName ``" -ForegroundColor White
Write-Host "    --name 'GoogleDriveCredentials' ``" -ForegroundColor White
Write-Host "    --value `"`$credentialsJson`"" -ForegroundColor White
Write-Host ""

Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Gray
Write-Host "5. ACTUALIZAR CONTAINER APPS CON SECRETOS" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Gray
Write-Host ""
Write-Host "Después de configurar los secretos, actualiza las Container Apps:" -ForegroundColor Yellow
Write-Host ""
Write-Host "  # Reiniciar Backend para cargar los secretos" -ForegroundColor Green
Write-Host "  az containerapp revision restart ``" -ForegroundColor White
Write-Host "    --name $backendAppName ``" -ForegroundColor White
Write-Host "    --resource-group $ResourceGroupName" -ForegroundColor White
Write-Host ""

Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Gray
Write-Host "6. CONFIGURAR URLS DE REDIRECCIONAMIENTO OAUTH" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Gray
Write-Host ""
Write-Host "Agrega estas URLs en Google Cloud Console:" -ForegroundColor Yellow
Write-Host "  → https://console.cloud.google.com/apis/credentials" -ForegroundColor White
Write-Host ""
Write-Host "URLs autorizadas de JavaScript:" -ForegroundColor Green
Write-Host "  • $($deployment.properties.outputs.frontendUrl.value)" -ForegroundColor White
Write-Host ""
Write-Host "URLs de redireccionamiento autorizadas:" -ForegroundColor Green
Write-Host "  • $($deployment.properties.outputs.backendUrl.value)/api/auth/google/callback" -ForegroundColor White
Write-Host "  • $($deployment.properties.outputs.frontendUrl.value)/auth/callback" -ForegroundColor White
Write-Host ""

Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  📋 RESUMEN DE COMANDOS" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════`n" -ForegroundColor Cyan

Write-Host "Guarda este script para configurar todos los secretos:" -ForegroundColor Yellow
Write-Host ""
Write-Host @"
# ============================================
# SCRIPT DE CONFIGURACIÓN DE SECRETOS
# ============================================

# 1. Generar JWT Secret Key
`$jwtKey = -join ((48..57) + (65..90) + (97..122) | Get-Random -Count 64 | % {[char]`$_})

# 2. Configurar todos los secretos
az keyvault secret set --vault-name $keyVaultName --name 'GoogleOAuthClientId' --value 'TU_CLIENT_ID'
az keyvault secret set --vault-name $keyVaultName --name 'GoogleOAuthClientSecret' --value 'TU_CLIENT_SECRET'
az keyvault secret set --vault-name $keyVaultName --name 'JwtSecretKey' --value "`$jwtKey"
az keyvault secret set --vault-name $keyVaultName --name 'GoogleDriveFolderId' --value 'TU_FOLDER_ID'

# 3. Reiniciar Backend
az containerapp revision restart --name $backendAppName --resource-group $ResourceGroupName

Write-Host \"✓ Secretos configurados exitosamente\" -ForegroundColor Green
"@ -ForegroundColor White

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host "Para actualizar la aplicación en el futuro, ejecuta:" -ForegroundColor Yellow
Write-Host "  ./deploy-azure.ps1 -ResourceGroupName $ResourceGroupName -Environment $Environment" -ForegroundColor White
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host ""
