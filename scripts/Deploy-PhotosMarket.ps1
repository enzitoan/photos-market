<#
.SYNOPSIS
    Script de despliegue para Photos Market en Azure Container Apps

.DESCRIPTION
    Este script replica la funcionalidad de los GitHub Actions para desplegar
    el backend y/o frontend de Photos Market a Azure Container Apps.
    Realiza build de imágenes Docker, push a ACR y actualización de Container Apps.

.PARAMETER Component
    Componente a desplegar: 'Backend', 'Frontend', o 'All' (default: 'All')

.PARAMETER ResourceGroupName
    Nombre del Resource Group de Azure (default: 'rg-photosmarket-dev')

.PARAMETER Environment
    Ambiente de despliegue: 'dev', 'staging', 'prod' (default: 'dev')

.PARAMETER SkipBuild
    Si se especifica, omite el build y push de imágenes Docker

.PARAMETER ImageTag
    Tag personalizado para las imágenes Docker (default: timestamp)

.EXAMPLE
    .\Deploy-PhotosMarket.ps1
    Despliega backend y frontend con configuración por defecto

.EXAMPLE
    .\Deploy-PhotosMarket.ps1 -Component Backend -SkipBuild
    Despliega solo el backend sin hacer build de la imagen

.EXAMPLE
    .\Deploy-PhotosMarket.ps1 -Environment prod -ImageTag "v1.2.3"
    Despliega con tag específico en ambiente de producción
#>

[CmdletBinding()]
param(
    [Parameter()]
    [ValidateSet('Backend', 'Frontend', 'All')]
    [string]$Component = 'All',

    [Parameter()]
    [string]$ResourceGroupName = 'rg-photosmarket-dev',

    [Parameter()]
    [ValidateSet('dev', 'staging', 'prod')]
    [string]$Environment = 'dev',

    [Parameter()]
    [switch]$SkipBuild,

    [Parameter()]
    [string]$ImageTag
)

# Variables de configuración
$config = @{
    ResourceGroup = $ResourceGroupName
    ContainerRegistry = 'photosmarketacrdev'
    BackendImageName = 'photosmarket-backend'
    BackendContainerApp = "photosmarket-backend-$Environment"
    FrontendImageName = 'photosmarket-frontend'
    FrontendContainerApp = "photosmarket-frontend-$Environment"
    Region = 'eastus'
}

# Generar tag si no se proporciona
if (-not $ImageTag) {
    $ImageTag = Get-Date -Format "yyyyMMddHHmmss"
}

# Colores para output
function Write-Info { param($Message) Write-Host "ℹ️  $Message" -ForegroundColor Cyan }
function Write-Success { param($Message) Write-Host "✅ $Message" -ForegroundColor Green }
function Write-Error { param($Message) Write-Host "❌ $Message" -ForegroundColor Red }
function Write-Step { param($Message) Write-Host "`n🔄 $Message" -ForegroundColor Yellow }

# Función para verificar que Azure CLI está instalado
function Test-AzureCLI {
    try {
        $azVersion = az version --output json 2>$null | ConvertFrom-Json
        Write-Info "Azure CLI version: $($azVersion.'azure-cli')"
        return $true
    }
    catch {
        Write-Error "Azure CLI no está instalado. Por favor instálalo desde: https://docs.microsoft.com/cli/azure/install-azure-cli"
        return $false
    }
}

# Función para verificar que Docker está instalado y corriendo
function Test-Docker {
    try {
        $dockerVersion = docker --version 2>$null
        Write-Info "Docker version: $dockerVersion"
        
        # Verificar que Docker daemon está corriendo
        docker ps 2>$null | Out-Null
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Docker daemon no está corriendo. Por favor inicia Docker Desktop."
            return $false
        }
        return $true
    }
    catch {
        Write-Error "Docker no está instalado o no está corriendo."
        return $false
    }
}

# Función para verificar login en Azure
function Test-AzureLogin {
    Write-Step "Verificando login en Azure..."
    $account = az account show 2>$null | ConvertFrom-Json
    
    if (-not $account) {
        Write-Info "No estás logueado en Azure. Iniciando login..."
        az login
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Error al hacer login en Azure"
            return $false
        }
    }
    
    Write-Success "Logueado en Azure como: $($account.user.name)"
    Write-Info "Subscription: $($account.name) ($($account.id))"
    return $true
}

# Función para hacer login en ACR
function Connect-AzureContainerRegistry {
    param([string]$RegistryName)
    
    Write-Step "Conectando a Azure Container Registry: $RegistryName..."
    az acr login --name $RegistryName
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Error al conectar con Azure Container Registry"
        return $false
    }
    
    Write-Success "Conectado a ACR: $RegistryName"
    return $true
}

# Función para build y push de imagen Docker
function Build-AndPushDockerImage {
    param(
        [string]$ComponentName,
        [string]$ContextPath,
        [string]$ImageName,
        [string]$Registry,
        [string]$Tag
    )
    
    Write-Step "Building Docker image para $ComponentName..."
    
    $imageTagFull = "$Registry.azurecr.io/${ImageName}:$Tag"
    $imageLatest = "$Registry.azurecr.io/${ImageName}:latest"
    
    Write-Info "Imagen: $imageTagFull"
    Write-Info "Context: $ContextPath"
    
    # Build
    docker build -t $imageTagFull -t $imageLatest $ContextPath
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Error al hacer build de la imagen Docker para $ComponentName"
        return $false
    }
    
    Write-Success "Build completado para $ComponentName"
    
    # Push con tag específico
    Write-Info "Pushing imagen con tag: $Tag..."
    docker push $imageTagFull
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Error al hacer push de la imagen con tag: $Tag"
        return $false
    }
    
    # Push latest
    Write-Info "Pushing imagen con tag: latest..."
    docker push $imageLatest
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Error al hacer push de la imagen con tag: latest"
        return $false
    }
    
    Write-Success "Push completado para $ComponentName"
    return $true
}

# Función para obtener URL de un Container App
function Get-ContainerAppUrl {
    param(
        [string]$AppName,
        [string]$ResourceGroup
    )
    
    Write-Info "Obteniendo URL de $AppName..."
    
    $fqdn = az containerapp show `
        --name $AppName `
        --resource-group $ResourceGroup `
        --query "properties.configuration.ingress.fqdn" `
        --output tsv 2>$null
    
    if ($LASTEXITCODE -ne 0 -or -not $fqdn) {
        Write-Error "No se pudo obtener la URL de $AppName"
        return $null
    }
    
    Write-Success "URL de ${AppName}: https://$fqdn"
    return $fqdn
}

# Función para desplegar Backend
function Deploy-Backend {
    param(
        [string]$Registry,
        [string]$ImageName,
        [string]$Tag,
        [string]$ContainerApp,
        [string]$ResourceGroup
    )
    
    Write-Step "Desplegando Backend a Azure Container Apps..."
    
    # Obtener URL del frontend
    $frontendFqdn = Get-ContainerAppUrl -AppName $config.FrontendContainerApp -ResourceGroup $ResourceGroup
    
    if (-not $frontendFqdn) {
        Write-Error "No se pudo obtener la URL del frontend. Continuando sin configurar FRONTEND_URL..."
        $envVars = @()
    }
    else {
        $envVars = @("FRONTEND_URL=https://$frontendFqdn")
    }
    
    $imageFullPath = "$Registry.azurecr.io/${ImageName}:$Tag"
    Write-Info "Actualizando Container App: $ContainerApp"
    Write-Info "Imagen: $imageFullPath"
    
    if ($envVars.Count -gt 0) {
        az containerapp update `
            --name $ContainerApp `
            --resource-group $ResourceGroup `
            --image $imageFullPath `
            --set-env-vars $envVars
    }
    else {
        az containerapp update `
            --name $ContainerApp `
            --resource-group $ResourceGroup `
            --image $imageFullPath
    }
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Error al desplegar Backend"
        return $false
    }
    
    # Verificar despliegue
    $revision = az containerapp show `
        --name $ContainerApp `
        --resource-group $ResourceGroup `
        --query "properties.latestRevisionName" `
        --output tsv
    
    Write-Success "Backend desplegado exitosamente!"
    Write-Info "Revision: $revision"
    
    return $true
}

# Función para desplegar Frontend
function Deploy-Frontend {
    param(
        [string]$Registry,
        [string]$ImageName,
        [string]$Tag,
        [string]$ContainerApp,
        [string]$ResourceGroup
    )
    
    Write-Step "Desplegando Frontend a Azure Container Apps..."
    
    # Obtener URL del backend
    $backendFqdn = Get-ContainerAppUrl -AppName $config.BackendContainerApp -ResourceGroup $ResourceGroup
    
    if (-not $backendFqdn) {
        Write-Error "No se pudo obtener la URL del backend. Continuando sin configurar VITE_API_URL..."
        $envVars = @()
    }
    else {
        $envVars = @("VITE_API_URL=https://$backendFqdn")
    }
    
    $imageFullPath = "$Registry.azurecr.io/${ImageName}:$Tag"
    Write-Info "Actualizando Container App: $ContainerApp"
    Write-Info "Imagen: $imageFullPath"
    
    if ($envVars.Count -gt 0) {
        az containerapp update `
            --name $ContainerApp `
            --resource-group $ResourceGroup `
            --image $imageFullPath `
            --set-env-vars $envVars
    }
    else {
        az containerapp update `
            --name $ContainerApp `
            --resource-group $ResourceGroup `
            --image $imageFullPath
    }
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Error al desplegar Frontend"
        return $false
    }
    
    # Verificar despliegue
    $revision = az containerapp show `
        --name $ContainerApp `
        --resource-group $ResourceGroup `
        --query "properties.latestRevisionName" `
        --output tsv
    
    Write-Success "Frontend desplegado exitosamente!"
    Write-Info "Revision: $revision"
    
    return $true
}

# ====================
# SCRIPT PRINCIPAL
# ====================

Write-Host "`n╔════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║          Photos Market - Azure Deployment Script          ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan

Write-Info "Component: $Component"
Write-Info "Environment: $Environment"
Write-Info "Resource Group: $($config.ResourceGroup)"
Write-Info "Image Tag: $ImageTag"
Write-Info "Skip Build: $SkipBuild"

# Verificaciones previas
if (-not (Test-AzureCLI)) { exit 1 }

if (-not $SkipBuild) {
    if (-not (Test-Docker)) { exit 1 }
}

if (-not (Test-AzureLogin)) { exit 1 }

# Obtener directorio raíz del proyecto
$scriptPath = Split-Path -Parent $PSCommandPath
$projectRoot = Split-Path -Parent $scriptPath

Write-Info "Project Root: $projectRoot"

# Build y Push de imágenes (si no se omite)
if (-not $SkipBuild) {
    # Login a ACR
    if (-not (Connect-AzureContainerRegistry -RegistryName $config.ContainerRegistry)) {
        exit 1
    }
    
    # Build Backend
    if ($Component -eq 'Backend' -or $Component -eq 'All') {
        $backendPath = Join-Path $projectRoot "src\backend"
        $success = Build-AndPushDockerImage `
            -ComponentName "Backend" `
            -ContextPath $backendPath `
            -ImageName $config.BackendImageName `
            -Registry $config.ContainerRegistry `
            -Tag $ImageTag
        
        if (-not $success) { exit 1 }
    }
    
    # Build Frontend
    if ($Component -eq 'Frontend' -or $Component -eq 'All') {
        $frontendPath = Join-Path $projectRoot "src\frontend"
        $success = Build-AndPushDockerImage `
            -ComponentName "Frontend" `
            -ContextPath $frontendPath `
            -ImageName $config.FrontendImageName `
            -Registry $config.ContainerRegistry `
            -Tag $ImageTag
        
        if (-not $success) { exit 1 }
    }
}
else {
    Write-Info "Omitiendo build de imágenes Docker (SkipBuild activado)"
}

# Despliegue a Azure Container Apps
Write-Host "`n" -NoNewline

# Deploy Backend
if ($Component -eq 'Backend' -or $Component -eq 'All') {
    $success = Deploy-Backend `
        -Registry $config.ContainerRegistry `
        -ImageName $config.BackendImageName `
        -Tag $ImageTag `
        -ContainerApp $config.BackendContainerApp `
        -ResourceGroup $config.ResourceGroup
    
    if (-not $success) { exit 1 }
}

# Deploy Frontend
if ($Component -eq 'Frontend' -or $Component -eq 'All') {
    $success = Deploy-Frontend `
        -Registry $config.ContainerRegistry `
        -ImageName $config.FrontendImageName `
        -Tag $ImageTag `
        -ContainerApp $config.FrontendContainerApp `
        -ResourceGroup $config.ResourceGroup
    
    if (-not $success) { exit 1 }
}

# Resumen final
Write-Host "`n╔════════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║              ¡Despliegue Completado Exitosamente!          ║" -ForegroundColor Green
Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Green

if ($Component -eq 'Backend' -or $Component -eq 'All') {
    $backendUrl = Get-ContainerAppUrl -AppName $config.BackendContainerApp -ResourceGroup $config.ResourceGroup
    if ($backendUrl) {
        Write-Host "`n🔗 Backend URL: " -NoNewline -ForegroundColor White
        Write-Host "https://$backendUrl" -ForegroundColor Cyan
    }
}

if ($Component -eq 'Frontend' -or $Component -eq 'All') {
    $frontendUrl = Get-ContainerAppUrl -AppName $config.FrontendContainerApp -ResourceGroup $config.ResourceGroup
    if ($frontendUrl) {
        Write-Host "🔗 Frontend URL: " -NoNewline -ForegroundColor White
        Write-Host "https://$frontendUrl" -ForegroundColor Cyan
    }
}

Write-Host ""
