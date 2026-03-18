<#
.SYNOPSIS
    Script rápido para actualizar URLs entre servicios de Photos Market

.DESCRIPTION
    Actualiza las variables de entorno FRONTEND_URL y VITE_API_URL
    entre los Container Apps sin hacer rebuild ni deployment de imágenes.

.PARAMETER ResourceGroupName
    Nombre del Resource Group de Azure (default: 'rg-photosmarket-dev')

.PARAMETER Environment
    Ambiente: 'dev', 'staging', 'prod' (default: 'dev')

.EXAMPLE
    .\Update-ServiceUrls.ps1
    Actualiza las URLs en ambiente dev

.EXAMPLE
    .\Update-ServiceUrls.ps1 -Environment prod
    Actualiza las URLs en ambiente prod
#>

[CmdletBinding()]
param(
    [Parameter()]
    [string]$ResourceGroupName = 'rg-photosmarket-dev',

    [Parameter()]
    [ValidateSet('dev', 'staging', 'prod')]
    [string]$Environment = 'dev'
)

$config = @{
    ResourceGroup = $ResourceGroupName
    BackendContainerApp = "photosmarket-backend-$Environment"
    FrontendContainerApp = "photosmarket-frontend-$Environment"
}

function Write-Info { param($Message) Write-Host "ℹ️  $Message" -ForegroundColor Cyan }
function Write-Success { param($Message) Write-Host "✅ $Message" -ForegroundColor Green }
function Write-Error { param($Message) Write-Host "❌ $Message" -ForegroundColor Red }

Write-Host "`n🔄 Actualizando URLs de servicios..." -ForegroundColor Yellow

# Obtener URL del backend
Write-Info "Obteniendo URL del Backend..."
$backendFqdn = az containerapp show `
    --name $config.BackendContainerApp `
    --resource-group $config.ResourceGroup `
    --query "properties.configuration.ingress.fqdn" `
    --output tsv 2>$null

if (-not $backendFqdn) {
    Write-Error "No se pudo obtener la URL del Backend"
    exit 1
}

Write-Success "Backend URL: https://$backendFqdn"

# Obtener URL del frontend
Write-Info "Obteniendo URL del Frontend..."
$frontendFqdn = az containerapp show `
    --name $config.FrontendContainerApp `
    --resource-group $config.ResourceGroup `
    --query "properties.configuration.ingress.fqdn" `
    --output tsv 2>$null

if (-not $frontendFqdn) {
    Write-Error "No se pudo obtener la URL del Frontend"
    exit 1
}

Write-Success "Frontend URL: https://$frontendFqdn"

# Actualizar Backend con Frontend URL
Write-Info "Actualizando Backend con FRONTEND_URL..."
az containerapp update `
    --name $config.BackendContainerApp `
    --resource-group $config.ResourceGroup `
    --set-env-vars "FRONTEND_URL=https://$frontendFqdn" `
    --output none

if ($LASTEXITCODE -eq 0) {
    Write-Success "Backend actualizado"
} else {
    Write-Error "Error al actualizar Backend"
}

# Actualizar Frontend con Backend URL
Write-Info "Actualizando Frontend con VITE_API_URL..."
az containerapp update `
    --name $config.FrontendContainerApp `
    --resource-group $config.ResourceGroup `
    --set-env-vars "VITE_API_URL=https://$backendFqdn" `
    --output none

if ($LASTEXITCODE -eq 0) {
    Write-Success "Frontend actualizado"
} else {
    Write-Error "Error al actualizar Frontend"
}

Write-Host "`n✅ URLs actualizadas correctamente!`n" -ForegroundColor Green
