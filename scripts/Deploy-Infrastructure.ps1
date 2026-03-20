<#
.SYNOPSIS
    Despliegue de infraestructura de Photos Market en Azure

.DESCRIPTION
    Este script despliega o actualiza la infraestructura completa de Photos Market en Azure:
    - Container Apps Environment
    - Azure Container Registry
    - Cosmos DB
    - Key Vault
    - Log Analytics
    - Container Apps (Backend y Frontend)
    
    Incluye validación de Bicep y verificación de configuración OAuth.

.PARAMETER ResourceGroupName
    Nombre del Resource Group de Azure (default: 'rg-photosmarket-dev')

.PARAMETER Location
    Región de Azure (default: 'eastus')

.PARAMETER Environment
    Ambiente de despliegue: 'dev', 'staging', 'prod' (default: 'dev')

.PARAMETER ValidationOnly
    Si se especifica, solo valida el template de Bicep sin desplegar

.PARAMETER SkipOAuthVerification
    Si se especifica, omite la verificación de OAuth después del despliegue

.EXAMPLE
    .\Deploy-Infrastructure.ps1
    Despliega infraestructura en ambiente dev con configuración por defecto

.EXAMPLE
    .\Deploy-Infrastructure.ps1 -ValidationOnly
    Solo valida el template de Bicep sin desplegar

.EXAMPLE
    .\Deploy-Infrastructure.ps1 -Environment prod -Location westus
    Despliega en ambiente de producción en región West US

.NOTES
    Requiere:
    - Azure CLI instalado y configurado
    - Permisos de Contributor en la subscripción de Azure
    - Archivos Bicep en carpeta infra/
    - Secretos configurados en Key Vault o variables de entorno
#>

[CmdletBinding()]
param(
    [Parameter()]
    [string]$ResourceGroupName = 'rg-photosmarket-dev',

    [Parameter()]
    [string]$Location = 'eastus',

    [Parameter()]
    [ValidateSet('dev', 'staging', 'prod')]
    [string]$Environment = 'dev',

    [Parameter()]
    [switch]$ValidationOnly,

    [Parameter()]
    [switch]$SkipOAuthVerification
)

$ErrorActionPreference = 'Stop'

# ============================================================================
# FUNCIONES DE UTILIDAD
# ============================================================================

function Write-Section { 
    param($Message) 
    Write-Host "`n╔═══════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
    Write-Host "║  $Message" -ForegroundColor Cyan
    Write-Host "╚═══════════════════════════════════════════════════════════════╝`n" -ForegroundColor Cyan 
}

function Write-Info { param($Message) Write-Host "ℹ️  $Message" -ForegroundColor Cyan }
function Write-Success { param($Message) Write-Host "✅ $Message" -ForegroundColor Green }
function Write-Warning { param($Message) Write-Host "⚠️  $Message" -ForegroundColor Yellow }
function Write-Error { param($Message) Write-Host "❌ $Message" -ForegroundColor Red }

# ============================================================================
# VALIDACIONES PREVIAS
# ============================================================================

Write-Section "PHOTOS MARKET - DESPLIEGUE DE INFRAESTRUCTURA"

Write-Host "Configuración:" -ForegroundColor Yellow
Write-Host "  Resource Group: $ResourceGroupName" -ForegroundColor White
Write-Host "  Location:       $Location" -ForegroundColor White
Write-Host "  Environment:    $Environment" -ForegroundColor White
Write-Host "  Validation:     $(if ($ValidationOnly) { 'Solo validación' } else { 'Despliegue completo' })" -ForegroundColor White
Write-Host ""

# Verificar Azure CLI
Write-Info "Verificando Azure CLI..."
try {
    $azVersion = az version --output json 2>$null | ConvertFrom-Json
    Write-Success "Azure CLI version: $($azVersion.'azure-cli')"
} catch {
    Write-Error "Azure CLI no está instalado"
    Write-Host "Instala Azure CLI desde: https://docs.microsoft.com/cli/azure/install-azure-cli" -ForegroundColor White
    exit 1
}

# Verificar login en Azure
Write-Info "Verificando autenticación en Azure..."
$account = az account show 2>$null | ConvertFrom-Json
if (-not $account) {
    Write-Info "No estás autenticado. Iniciando login..."
    az login
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Error al autenticar en Azure"
        exit 1
    }
    $account = az account show | ConvertFrom-Json
}
Write-Success "Autenticado como: $($account.user.name)"
Write-Info "Subscription: $($account.name)"

# Obtener directorio del proyecto
$scriptPath = Split-Path -Parent $PSCommandPath
$projectRoot = Split-Path -Parent $scriptPath
$infraPath = Join-Path $projectRoot "infra"

Write-Info "Directorio del proyecto: $projectRoot"

# Verificar archivos Bicep
Write-Info "Verificando archivos de infraestructura..."
$bicepMain = Join-Path $infraPath "main.bicep"
$bicepParam = Join-Path $infraPath "main.bicepparam"

if (-not (Test-Path $bicepMain)) {
    Write-Error "No se encontró el archivo: $bicepMain"
    exit 1
}

if (-not (Test-Path $bicepParam)) {
    Write-Error "No se encontró el archivo: $bicepParam"
    exit 1
}

Write-Success "Archivos de infraestructura encontrados"

# ============================================================================
# ADVERTENCIA SOBRE SECRETOS
# ============================================================================

Write-Section "CONFIGURACIÓN DE SECRETOS"

Write-Host "Este script despliega la infraestructura. Los secretos pueden ser:" -ForegroundColor Yellow
Write-Host "  1. Variables de entorno (para Bicep usar readEnvironmentVariable())" -ForegroundColor White
Write-Host "  2. Valores en main.bicepparam" -ForegroundColor White
Write-Host "  3. Key Vault existente" -ForegroundColor White
Write-Host ""
Write-Host "Secretos requeridos:" -ForegroundColor Cyan
Write-Host "  • GOOGLE_OAUTH_CLIENT_ID" -ForegroundColor White
Write-Host "  • GOOGLE_OAUTH_CLIENT_SECRET" -ForegroundColor White
Write-Host "  • JWT_SECRET_KEY" -ForegroundColor White
Write-Host "  • GOOGLE_DRIVE_ROOT_FOLDER_ID" -ForegroundColor White
Write-Host "  • GOOGLE_DRIVE_CREDENTIALS (JSON)" -ForegroundColor White
Write-Host ""

# Verificar si hay variables de entorno configuradas
$envVarsConfigured = @()
$envVarsMissing = @()

$requiredEnvVars = @(
    'GOOGLE_OAUTH_CLIENT_ID',
    'GOOGLE_OAUTH_CLIENT_SECRET',
    'JWT_SECRET_KEY',
    'GOOGLE_DRIVE_ROOT_FOLDER_ID',
    'GOOGLE_DRIVE_CREDENTIALS'
)

foreach ($varName in $requiredEnvVars) {
    if ([Environment]::GetEnvironmentVariable($varName)) {
        $envVarsConfigured += $varName
    } else {
        $envVarsMissing += $varName
    }
}

if ($envVarsConfigured.Count -gt 0) {
    Write-Success "Variables de entorno configuradas ($($envVarsConfigured.Count)):"
    foreach ($var in $envVarsConfigured) {
        Write-Host "  ✓ $var" -ForegroundColor Green
    }
    Write-Host ""
}

if ($envVarsMissing.Count -gt 0) {
    Write-Warning "Variables de entorno NO configuradas ($($envVarsMissing.Count)):"
    foreach ($var in $envVarsMissing) {
        Write-Host "  ✗ $var" -ForegroundColor Yellow
    }
    Write-Host ""
    Write-Host "Si tu archivo main.bicepparam usa readEnvironmentVariable(), configura estas variables:" -ForegroundColor Yellow
    Write-Host ""
    foreach ($var in $envVarsMissing) {
        Write-Host "`$env:$var = 'TU_VALOR_AQUI'" -ForegroundColor Gray
    }
    Write-Host ""
}

# Pedir confirmación
if (-not $ValidationOnly) {
    Write-Host "¿Continuar con el despliegue? (S/N): " -ForegroundColor Yellow -NoNewline
    $confirmation = Read-Host
    if ($confirmation -ne 'S' -and $confirmation -ne 's') {
        Write-Warning "Despliegue cancelado por el usuario"
        exit 0
    }
}

# ============================================================================
# CREAR RESOURCE GROUP
# ============================================================================

Write-Section "PREPARANDO RESOURCE GROUP"

Write-Info "Creando/verificando Resource Group: $ResourceGroupName"
az group create --name $ResourceGroupName --location $Location --output none

if ($LASTEXITCODE -ne 0) {
    Write-Error "Error al crear Resource Group"
    exit 1
}

Write-Success "Resource Group listo: $ResourceGroupName"

# ============================================================================
# VALIDAR BICEP TEMPLATE
# ============================================================================

Write-Section "VALIDANDO TEMPLATE DE BICEP"

Write-Info "Validando sintaxis y configuración de Bicep..."

$validationOutput = az deployment group validate `
    --resource-group $ResourceGroupName `
    --template-file $bicepMain `
    --parameters $bicepParam `
    --parameters environmentName=$Environment `
    --output json 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Error "Error en la validación del template de Bicep"
    Write-Host $validationOutput -ForegroundColor Red
    exit 1
}

Write-Success "Template de Bicep válido"

# ============================================================================
# DESPLEGAR INFRAESTRUCTURA
# ============================================================================

if ($ValidationOnly) {
    Write-Section "VALIDACIÓN COMPLETADA"
    Write-Success "El template de Bicep es válido"
    Write-Success "Todos los archivos necesarios están presentes"
    Write-Host ""
    Write-Info "Para desplegar la infraestructura, ejecuta sin -ValidationOnly"
    exit 0
}

Write-Section "DESPLEGANDO INFRAESTRUCTURA"

$deploymentName = "infra-deployment-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
Write-Info "Nombre del deployment: $deploymentName"
Write-Info "Iniciando despliegue (esto puede tomar varios minutos)..."

# Realizar el despliegue
az deployment group create `
    --resource-group $ResourceGroupName `
    --template-file $bicepMain `
    --parameters $bicepParam `
    --parameters environmentName=$Environment `
    --name $deploymentName `
    --output none

if ($LASTEXITCODE -ne 0) {
    Write-Error "Error al desplegar infraestructura"
    Write-Host ""
    Write-Host "Para ver detalles del error, ejecuta:" -ForegroundColor Yellow
    Write-Host "  az deployment group show --resource-group $ResourceGroupName --name $deploymentName" -ForegroundColor Gray
    exit 1
}

Write-Success "Infraestructura desplegada exitosamente"

# ============================================================================
Get-DEPLOYMENT OUTPUTS
# ============================================================================

Write-Section "OBTENIENDO INFORMACIÓN DE DEPLOYMENT"

Write-Info "Obteniendo outputs del deployment..."

$deployment = az deployment group show `
    --resource-group $ResourceGroupName `
    --name $deploymentName `
    --output json | ConvertFrom-Json

$outputs = $deployment.properties.outputs

# Extraer valores de outputs
$frontendUrl = if ($outputs.frontendUrl) { $outputs.frontendUrl.value } else { "" }
$backendUrl = if ($outputs.backendUrl) { $outputs.backendUrl.value } else { "" }
$acrLoginServer = if ($outputs.containerRegistryLoginServer) { $outputs.containerRegistryLoginServer.value } else { "" }
$cosmosDbEndpoint = if ($outputs.cosmosDbEndpoint) { $outputs.cosmosDbEndpoint.value } else { "" }
$keyVaultName = if ($outputs.keyVaultName) { $outputs.keyVaultName.value } else { "" }

# Extraer FQDNs (sin https://)
$frontendFqdn = $frontendUrl -replace 'https://', ''
$backendFqdn = $backendUrl -replace 'https://', ''

Write-Success "Outputs obtenidos correctamente"

# ============================================================================
# VERIFICAR OAUTH CONFIGURATION
# ============================================================================

if (-not $SkipOAuthVerification) {
    Write-Section "VERIFICANDO CONFIGURACIÓN OAUTH"

    $backendAppName = "photosmarket-backend-$Environment"
    
    Write-Info "Obteniendo configuración del Backend: $backendAppName"
    
    # Obtener FRONTEND_URL actual del Backend
    $currentFrontendUrl = az containerapp show `
        --name $backendAppName `
        --resource-group $ResourceGroupName `
        --query "properties.template.containers[0].env[?name=='FRONTEND_URL'].value" `
        --output tsv 2>$null

    $expectedFrontendUrl = "https://$frontendFqdn"

    Write-Host "  FRONTEND_URL actual:   $currentFrontendUrl" -ForegroundColor White
    Write-Host "  FRONTEND_URL esperado: $expectedFrontendUrl" -ForegroundColor White

    # Verificar si necesita actualización
    if ($currentFrontendUrl -ne $expectedFrontendUrl) {
        Write-Warning "FRONTEND_URL no coincide con el FQDN real del Frontend"
        Write-Info "Actualizando Backend con FRONTEND_URL correcto..."

        az containerapp update `
            --name $backendAppName `
            --resource-group $ResourceGroupName `
            --set-env-vars "FRONTEND_URL=$expectedFrontendUrl" `
            --output none

        if ($LASTEXITCODE -eq 0) {
            Write-Success "Backend actualizado con FRONTEND_URL correcto"
        } else {
            Write-Warning "Error al actualizar Backend"
        }
    } else {
        Write-Success "FRONTEND_URL es correcto"
    }

    # Verificar scopes de OAuth
    Write-Info "Verificando OAuth scopes..."
    
    $scopesJson = az containerapp show `
        --name $backendAppName `
        --resource-group $ResourceGroupName `
        --query "properties.template.containers[0].env[?contains(name, 'GoogleOAuth__Scopes')].{name:name, value:value}" `
        --output json 2>$null | ConvertFrom-Json

    $scopeCount = if ($scopesJson) { $scopesJson.Count } else { 0 }

    Write-Host "  Scopes configurados: $scopeCount" -ForegroundColor White

    if ($scopesJson -and $scopeCount -gt 0) {
        foreach ($scope in $scopesJson) {
            Write-Host "    ✓ $($scope.name): $($scope.value)" -ForegroundColor Green
        }
    }

    if ($scopeCount -lt 3) {
        Write-Warning "Se esperan 3 scopes de OAuth, pero solo hay $scopeCount configurados"
        Write-Host ""
        Write-Host "  Esto puede causar el error: 'Missing required parameter: scope'" -ForegroundColor Yellow
        Write-Host "  Para corregirlo, ejecuta:" -ForegroundColor Yellow
        Write-Host "    .\scripts\Fix-AzureOAuthConfig.ps1 -ResourceGroupName $ResourceGroupName -Environment $Environment" -ForegroundColor Cyan
    } else {
        Write-Success "OAuth scopes configurados correctamente"
    }
}

# ============================================================================
# SUMMARY
# ============================================================================

Write-Section "DESPLIEGUE COMPLETADO"

Write-Host "🌐 URLs de la Aplicación:" -ForegroundColor Cyan
Write-Host "  Frontend: " -NoNewline -ForegroundColor White
Write-Host $frontendUrl -ForegroundColor Green
Write-Host "  Backend:  " -NoNewline -ForegroundColor White
Write-Host $backendUrl -ForegroundColor Green
Write-Host ""

Write-Host "📦 Recursos Desplegados:" -ForegroundColor Cyan
Write-Host "  Container Registry: $acrLoginServer" -ForegroundColor White
Write-Host "  Cosmos DB:          $cosmosDbEndpoint" -ForegroundColor White
Write-Host "  Key Vault:          $keyVaultName" -ForegroundColor White
Write-Host ""

Write-Host "🔐 Configuración OAuth:" -ForegroundColor Cyan
Write-Host "  Frontend FQDN:  $frontendFqdn" -ForegroundColor White
Write-Host "  Redirect URI:   https://${frontendFqdn}/callback" -ForegroundColor Green
Write-Host ""

# Listar Container Apps
Write-Host "📊 Container Apps:" -ForegroundColor Cyan
az containerapp list `
    --resource-group $ResourceGroupName `
    --query "[].{Name:name, FQDN:properties.configuration.ingress.fqdn, Status:properties.runningStatus}" `
    --output table

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Yellow
Write-Host "  PRÓXIMOS PASOS" -ForegroundColor Yellow
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Yellow
Write-Host ""

Write-Host "1️⃣  Configurar Google Cloud Console:" -ForegroundColor Cyan
Write-Host "   • Ve a: https://console.cloud.google.com/" -ForegroundColor White
Write-Host "   • Navega a: APIs & Services → Credentials" -ForegroundColor White
Write-Host "   • En 'Authorized redirect URIs', agrega:" -ForegroundColor White
Write-Host "     https://${frontendFqdn}/callback" -ForegroundColor Green
Write-Host ""

Write-Host "2️⃣  Verificar OAuth Configuration:" -ForegroundColor Cyan
Write-Host "   • Si tienes errores de OAuth, ejecuta:" -ForegroundColor White
Write-Host "     .\scripts\Fix-AzureOAuthConfig.ps1 -ResourceGroupName $ResourceGroupName" -ForegroundColor Cyan
Write-Host ""

Write-Host "3️⃣  Desplegar las Aplicaciones:" -ForegroundColor Cyan
Write-Host "   • Para desplegar Backend y Frontend, ejecuta:" -ForegroundColor White
Write-Host "     .\scripts\Deploy-PhotosMarket.ps1 -ResourceGroupName $ResourceGroupName" -ForegroundColor Cyan
Write-Host ""

Write-Host "4️⃣  Sincronizar URLs entre servicios:" -ForegroundColor Cyan
Write-Host "   • Si las URLs cambian, ejecuta:" -ForegroundColor White
Write-Host "     .\scripts\Update-ServiceUrls.ps1 -ResourceGroupName $ResourceGroupName" -ForegroundColor Cyan
Write-Host ""

Write-Success "¡Infraestructura desplegada exitosamente!"
Write-Host ""
