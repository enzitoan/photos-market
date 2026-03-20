<#
.SYNOPSIS
    Script de despliegue para Photos Market en Azure Container Apps

.DESCRIPTION
    Este script despliega el backend y/o frontend de Photos Market a Azure Container Apps.
    Realiza build de imágenes Docker, push a ACR y actualización de Container Apps.
    
    IMPORTANTE: Despliega Frontend PRIMERO, luego Backend para asegurar que el Backend
    use el FQDN real del Frontend (corrige el error "Missing required parameter: scope")

.PARAMETER Component
    Componente a desplegar: 'Backend', 'Frontend', o 'Both' (default: 'Both')

.PARAMETER ResourceGroupName
    Nombre del Resource Group de Azure (default: 'rg-photosmarket-dev')

.PARAMETER Environment
    Ambiente de despliegue: 'dev', 'staging', 'prod' (default: 'dev')

.PARAMETER SkipBuild
    Si se especifica, omite el build y push de imágenes Docker

.PARAMETER ImageTag
    Tag personalizado para las imágenes Docker (default: timestamp)

.PARAMETER SkipOAuthVerification
    Si se especifica, omite la verificación de OAuth después del despliegue

.EXAMPLE
    .\Deploy-PhotosMarket.ps1
    Despliega backend y frontend con configuración por defecto

.EXAMPLE
    .\Deploy-PhotosMarket.ps1 -Component Backend -SkipBuild
    Despliega solo el backend sin hacer build de la imagen

.EXAMPLE
    .\Deploy-PhotosMarket.ps1 -Environment prod -ImageTag "v1.2.3"
    Despliega con tag específico en ambiente de producción

.NOTES
    Para corregir errores de OAuth, ejecuta después del deployment:
    .\Fix-AzureOAuthConfig.ps1 -ResourceGroupName <resource-group>
#>

[CmdletBinding()]
param(
    [Parameter()]
    [ValidateSet('Backend', 'Frontend', 'Both')]
    [string]$Component = 'Both',

    [Parameter()]
    [string]$ResourceGroupName = 'rg-photosmarket-dev',

    [Parameter()]
    [ValidateSet('dev', 'staging', 'prod')]
    [string]$Environment = 'dev',

    [Parameter()]
    [switch]$SkipBuild,

    [Parameter()]
    [string]$ImageTag,

    [Parameter()]
    [switch]$SkipOAuthVerification
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
        [string]$ResourceGroup,
        [string]$FrontendFqdn
    )
    
    Write-Step "Desplegando Backend a Azure Container Apps..."
    
    # Usar el Frontend FQDN proporcionado (real)
    if (-not $FrontendFqdn) {
        Write-Error "Frontend FQDN no proporcionado. No se puede configurar FRONTEND_URL correctamente"
        return $false
    }
    
    $frontendUrl = "https://$FrontendFqdn"
    $envVars = @("FRONTEND_URL=$frontendUrl")
    
    $imageFullPath = "$Registry.azurecr.io/${ImageName}:$Tag"
    Write-Info "Actualizando Container App: $ContainerApp"
    Write-Info "Imagen: $imageFullPath"
    Write-Info "FRONTEND_URL: $frontendUrl"
    
    az containerapp update `
        --name $ContainerApp `
        --resource-group $ResourceGroup `
        --image $imageFullPath `
        --set-env-vars $envVars `
        --output none
    
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
    Write-Success "FRONTEND_URL configurado: $frontendUrl"
    
    return $true
}

# Función para desplegar Frontend
function Deploy-Frontend {
    param(
        [string]$Registry,
        [string]$ImageName,
        [string]$Tag,
        [string]$ContainerApp,
        [string]$ResourceGroup,
        [string]$BackendFqdn
    )
    
    Write-Step "Desplegando Frontend a Azure Container Apps..."
    
    $imageFullPath = "$Registry.azurecr.io/${ImageName}:$Tag"
    Write-Info "Actualizando Container App: $ContainerApp"
    Write-Info "Imagen: $imageFullPath"
    
    # Si hay Backend FQDN, configurarlo
    if ($BackendFqdn) {
        $backendUrl = "https://$BackendFqdn"
        Write-Info "VITE_API_URL: $backendUrl"
        
        az containerapp update `
            --name $ContainerApp `
            --resource-group $ResourceGroup `
            --image $imageFullPath `
            --set-env-vars "VITE_API_URL=$backendUrl" `
            --output none
    }
    else {
        Write-Warning "Backend FQDN no disponible, desplegando sin VITE_API_URL"
        
        az containerapp update `
            --name $ContainerApp `
            --resource-group $ResourceGroup `
            --image $imageFullPath `
            --output none
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
Write-Info "Skip OAuth Verification: $SkipOAuthVerification"

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
    
    # Build Frontend primero (si aplica)
    if ($Component -eq 'Frontend' -or $Component -eq 'Both') {
        $frontendPath = Join-Path $projectRoot "src\frontend"
        $success = Build-AndPushDockerImage `
            -ComponentName "Frontend" `
            -ContextPath $frontendPath `
            -ImageName $config.FrontendImageName `
            -Registry $config.ContainerRegistry `
            -Tag $ImageTag
        
        if (-not $success) { exit 1 }
    }
    
    # Build Backend después (si aplica)
    if ($Component -eq 'Backend' -or $Component -eq 'Both') {
        $backendPath = Join-Path $projectRoot "src\backend"
        $success = Build-AndPushDockerImage `
            -ComponentName "Backend" `
            -ContextPath $backendPath `
            -ImageName $config.BackendImageName `
            -Registry $config.ContainerRegistry `
            -Tag $ImageTag
        
        if (-not $success) { exit 1 }
    }
}
else {
    Write-Info "Omitiendo build de imágenes Docker (SkipBuild activado)"
}

# ============================================================================
# DESPLIEGUE A AZURE CONTAINER APPS
# IMPORTANTE: Frontend PRIMERO, luego Backend (para OAuth correcto)
# ============================================================================

Write-Host "`n" -NoNewline

# Variables para almacenar FQDNs
$frontendFqdn = $null
$backendFqdn = $null

# PASO 1: Deploy Frontend primero
if ($Component -eq 'Frontend' -or $Component -eq 'Both') {
    # Obtener Backend FQDN actual (si existe) para configurar Frontend
    $backendFqdn = az containerapp show `
        --name $config.BackendContainerApp `
        --resource-group $config.ResourceGroup `
        --query "properties.configuration.ingress.fqdn" `
        --output tsv 2>$null
    
    if (-not $backendFqdn) {
        Write-Warning "Backend no encontrado, Frontend se desplegará sin VITE_API_URL"
    }
    
    $success = Deploy-Frontend `
        -Registry $config.ContainerRegistry `
        -ImageName $config.FrontendImageName `
        -Tag $ImageTag `
        -ContainerApp $config.FrontendContainerApp `
        -ResourceGroup $config.ResourceGroup `
        -BackendFqdn $backendFqdn
    
    if (-not $success) { exit 1 }
    
    # Obtener Frontend FQDN real después del deployment
    $frontendFqdn = az containerapp show `
        --name $config.FrontendContainerApp `
        --resource-group $config.ResourceGroup `
        --query "properties.configuration.ingress.fqdn" `
        --output tsv
    
    Write-Success "Frontend FQDN real: https://$frontendFqdn"
}

# PASO 2: Deploy Backend con Frontend FQDN real
if ($Component -eq 'Backend' -or $Component -eq 'Both') {
    # Obtener Frontend FQDN si no lo tenemos aún
    if (-not $frontendFqdn) {
        $frontendFqdn = az containerapp show `
            --name $config.FrontendContainerApp `
            --resource-group $config.ResourceGroup `
            --query "properties.configuration.ingress.fqdn" `
            --output tsv 2>$null
        
        if (-not $frontendFqdn) {
            Write-Error "No se pudo obtener Frontend FQDN. El Backend necesita esta URL para OAuth"
            exit 1
        }
    }
    
    $success = Deploy-Backend `
        -Registry $config.ContainerRegistry `
        -ImageName $config.BackendImageName `
        -Tag $ImageTag `
        -ContainerApp $config.BackendContainerApp `
        -ResourceGroup $config.ResourceGroup `
        -FrontendFqdn $frontendFqdn
    
    if (-not $success) { exit 1 }
    
    # Obtener Backend FQDN real
    $backendFqdn = az containerapp show `
        --name $config.BackendContainerApp `
        --resource-group $config.ResourceGroup `
        --query "properties.configuration.ingress.fqdn" `
        --output tsv
    
    Write-Success "Backend FQDN real: https://$backendFqdn"
}

# PASO 3: Verificar OAuth Configuration (si Backend fue desplegado)
if (($Component -eq 'Backend' -or $Component -eq 'Both') -and -not $SkipOAuthVerification) {
    Write-Host "`n" -NoNewline
    Write-Step "Verificando Configuración OAuth..."
    
    # Obtener FRONTEND_URL configurado en Backend
    $configuredFrontendUrl = az containerapp show `
        --name $config.BackendContainerApp `
        --resource-group $config.ResourceGroup `
        --query "properties.template.containers[0].env[?name=='FRONTEND_URL'].value" `
        --output tsv 2>$null
    
    Write-Info "FRONTEND_URL configurado: $configuredFrontendUrl"
    
    # Verificar OAuth Scopes
    $scopesJson = az containerapp show `
        --name $config.BackendContainerApp `
        --resource-group $config.ResourceGroup `
        --query "properties.template.containers[0].env[?contains(name, 'GoogleOAuth__Scopes')].{name:name, value:value}" `
        --output json 2>$null | ConvertFrom-Json
    
    $scopeCount = if ($scopesJson) { $scopesJson.Count } else { 0 }
    Write-Info "OAuth Scopes configurados: $scopeCount"
    
    if ($scopesJson -and $scopeCount -gt 0) {
        foreach ($scope in $scopesJson) {
            Write-Host "  ✓ $($scope.name): $($scope.value)" -ForegroundColor Green
        }
    }
    
    # Validar configuración
    $expectedFrontendUrl = "https://$frontendFqdn"
    
    if ($configuredFrontendUrl -ne $expectedFrontendUrl) {
        Write-Warning "FRONTEND_URL no coincide con el FQDN real del Frontend"
        Write-Host "  Configurado: $configuredFrontendUrl" -ForegroundColor Red
        Write-Host "  Esperado:    $expectedFrontendUrl" -ForegroundColor Green
    } else {
        Write-Success "FRONTEND_URL es correcto"
    }
    
    if ($scopeCount -lt 3) {
        Write-Warning "Se esperan 3 scopes de OAuth, pero solo hay $scopeCount configurados"
        Write-Host ""  -ForegroundColor Yellow
        Write-Host "  Esto puede causar el error: 'Missing required parameter: scope'" -ForegroundColor Yellow
        Write-Host "  Para corregirlo, ejecuta:" -ForegroundColor Yellow
        Write-Host "    .\scripts\Fix-AzureOAuthConfig.ps1 -ResourceGroupName $($config.ResourceGroup) -Environment $Environment" -ForegroundColor Cyan
    } else {
        Write-Success "OAuth configuration looks good!"
    }
}

# PASO 4: Actualizar Frontend con Backend URL final (si ambos fueron desplegados)
if ($Component -eq 'Both' -and $backendFqdn -and $frontendFqdn) {
    Write-Host "`n" -NoNewline
    Write-Step "Sincronizando URLs entre servicios..."
    
    Write-Info "Actualizando Frontend con Backend URL final..."
    
    az containerapp update `
        --name $config.FrontendContainerApp `
        --resource-group $config.ResourceGroup `
        --set-env-vars "VITE_API_URL=https://$backendFqdn" `
        --output none
    
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Frontend actualizado con Backend URL: https://$backendFqdn"
    } else {
        Write-Warning "Error al actualizar Frontend con Backend URL"
    }
}

# Resumen final
Write-Host "`n╔════════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║              ¡Despliegue Completado Exitosamente!          ║" -ForegroundColor Green
Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Green
Write-Host ""

# Mostrar URLs finales
if ($Component -eq 'Frontend' -or $Component -eq 'Both') {
    if (-not $frontendFqdn) {
        $frontendFqdn = az containerapp show `
            --name $config.FrontendContainerApp `
            --resource-group $config.ResourceGroup `
            --query "properties.configuration.ingress.fqdn" `
            --output tsv 2>$null
    }
    
    if ($frontendFqdn) {
        Write-Host "🔗 Frontend URL: " -NoNewline -ForegroundColor White
        Write-Host "https://$frontendFqdn" -ForegroundColor Cyan
    }
}

if ($Component -eq 'Backend' -or $Component -eq 'Both') {
    if (-not $backendFqdn) {
        $backendFqdn = az containerapp show `
            --name $config.BackendContainerApp `
            --resource-group $config.ResourceGroup `
            --query "properties.configuration.ingress.fqdn" `
            --output tsv 2>$null
    }
    
    if ($backendFqdn) {
        Write-Host "🔗 Backend URL:  " -NoNewline -ForegroundColor White
        Write-Host "https://$backendFqdn" -ForegroundColor Cyan
    }
}

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Yellow
Write-Host "  PRÓXIMOS PASOS" -ForegroundColor Yellow
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Yellow
Write-Host ""

if ($Component -eq 'Backend' -or $Component -eq 'Both') {
    Write-Host "1️⃣  Configurar Google Cloud Console:" -ForegroundColor Cyan
    Write-Host "   • Ve a: https://console.cloud.google.com/" -ForegroundColor White
    Write-Host "   • Navega a: APIs & Services → Credentials" -ForegroundColor White
    Write-Host "   • En 'Authorized redirect URIs', agrega:" -ForegroundColor White
    Write-Host "     https://${frontendFqdn}/callback" -ForegroundColor Green
    Write-Host ""
    
    Write-Host "2️⃣  Verificar OAuth (si hay errores):" -ForegroundColor Cyan
    Write-Host "   .\scripts\Fix-AzureOAuthConfig.ps1 -ResourceGroupName $($config.ResourceGroup)" -ForegroundColor White
    Write-Host ""
}

Write-Host "3️⃣  Ver logs en tiempo real:" -ForegroundColor Cyan
if ($Component -eq 'Backend' -or $Component -eq 'Both') {
    Write-Host "   az containerapp logs show --name $($config.BackendContainerApp) --resource-group $($config.ResourceGroup) --follow" -ForegroundColor White
}
if ($Component -eq 'Frontend' -or $Component -eq 'Both') {
    Write-Host "   az containerapp logs show --name $($config.FrontendContainerApp) --resource-group $($config.ResourceGroup) --follow" -ForegroundColor White
}

Write-Host ""
