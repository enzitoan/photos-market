<#
.SYNOPSIS
    Script para verificar el estado de los recursos de Photos Market en Azure

.DESCRIPTION
    Muestra información detallada sobre el estado de los Container Apps,
    Container Registry, y otras configuraciones relevantes.

.PARAMETER ResourceGroupName
    Nombre del Resource Group de Azure (default: 'rg-photosmarket-dev')

.PARAMETER Environment
    Ambiente: 'dev', 'staging', 'prod' (default: 'dev')

.PARAMETER ShowLogs
    Mostrar logs recientes de los servicios

.EXAMPLE
    .\Get-DeploymentStatus.ps1
    Muestra el estado de los recursos en dev

.EXAMPLE
    .\Get-DeploymentStatus.ps1 -ShowLogs
    Muestra el estado y logs recientes
#>

[CmdletBinding()]
param(
    [Parameter()]
    [string]$ResourceGroupName = 'rg-photosmarket-dev',

    [Parameter()]
    [ValidateSet('dev', 'staging', 'prod')]
    [string]$Environment = 'dev',

    [Parameter()]
    [switch]$ShowLogs
)

$config = @{
    ResourceGroup = $ResourceGroupName
    ContainerRegistry = 'photosmarketacrdev'
    BackendContainerApp = "photosmarket-backend-$Environment"
    FrontendContainerApp = "photosmarket-frontend-$Environment"
}

function Write-Header { 
    param($Message) 
    Write-Host "`n╔════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
    Write-Host "║  $($Message.PadRight(58))║" -ForegroundColor Cyan
    Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
}

function Write-Info { param($Label, $Value) 
    Write-Host "  $($Label): " -NoNewline -ForegroundColor Gray
    Write-Host $Value -ForegroundColor White
}

function Write-Status { 
    param($Label, $Value, $IsGood = $true) 
    $color = if ($IsGood) { "Green" } else { "Red" }
    $icon = if ($IsGood) { "✅" } else { "❌" }
    Write-Host "  $icon $($Label): " -NoNewline -ForegroundColor Gray
    Write-Host $Value -ForegroundColor $color
}

Write-Header "Photos Market - Deployment Status"

# Verificar Resource Group
Write-Host "`n📦 Resource Group" -ForegroundColor Yellow
$rgExists = az group exists --name $config.ResourceGroup

if ($rgExists -eq "true") {
    Write-Status "Resource Group" $config.ResourceGroup -IsGood $true
    $rg = az group show --name $config.ResourceGroup | ConvertFrom-Json
    Write-Info "Location" $rg.location
    Write-Info "ID" $rg.id
} else {
    Write-Status "Resource Group" "No existe" -IsGood $false
    exit 1
}

# Verificar Container Registry
Write-Host "`n🐳 Azure Container Registry" -ForegroundColor Yellow
try {
    $acr = az acr show --name $config.ContainerRegistry 2>$null | ConvertFrom-Json
    Write-Status "ACR" $config.ContainerRegistry -IsGood $true
    Write-Info "Login Server" $acr.loginServer
    Write-Info "SKU" $acr.sku.name
    Write-Info "Admin Enabled" $acr.adminUserEnabled
    
    # Listar imágenes
    Write-Host "`n  📋 Imágenes disponibles:" -ForegroundColor Gray
    $repos = az acr repository list --name $config.ContainerRegistry 2>$null | ConvertFrom-Json
    foreach ($repo in $repos) {
        $tags = az acr repository show-tags --name $config.ContainerRegistry --repository $repo --top 3 --orderby time_desc 2>$null | ConvertFrom-Json
        Write-Host "    • $repo" -ForegroundColor Cyan
        foreach ($tag in $tags) {
            Write-Host "      - $tag" -ForegroundColor DarkGray
        }
    }
} catch {
    Write-Status "ACR" "No encontrado o sin acceso" -IsGood $false
}

# Función para mostrar info de Container App
function Show-ContainerAppInfo {
    param($AppName, $DisplayName)
    
    Write-Host "`n🚀 $DisplayName" -ForegroundColor Yellow
    try {
        $app = az containerapp show --name $AppName --resource-group $config.ResourceGroup 2>$null | ConvertFrom-Json
        
        Write-Status "Estado" $app.properties.provisioningState
        Write-Info "Revision Actual" $app.properties.latestRevisionName
        Write-Info "FQDN" "https://$($app.properties.configuration.ingress.fqdn)"
        Write-Info "Imagen" $app.properties.template.containers[0].image
        
        # Replicas
        $replicas = $app.properties.template.scale
        Write-Info "Replicas (min-max)" "$($replicas.minReplicas)-$($replicas.maxReplicas)"
        
        # CPU y Memoria
        $resources = $app.properties.template.containers[0].resources
        Write-Info "CPU" $resources.cpu
        Write-Info "Memoria" $resources.memory
        
        # Variables de entorno (sin valores sensibles)
        Write-Host "`n  🔧 Variables de Entorno:" -ForegroundColor Gray
        $envVars = $app.properties.template.containers[0].env
        foreach ($env in $envVars) {
            if ($env.name -notmatch "(SECRET|KEY|PASSWORD|TOKEN)") {
                $value = if ($env.value) { $env.value } else { "(secret ref)" }
                Write-Host "    • $($env.name) = $value" -ForegroundColor DarkGray
            } else {
                Write-Host "    • $($env.name) = ********" -ForegroundColor DarkGray
            }
        }
        
        # Health probes
        if ($app.properties.template.containers[0].probes) {
            Write-Host "`n  ❤️  Health Probes:" -ForegroundColor Gray
            foreach ($probe in $app.properties.template.containers[0].probes) {
                Write-Host "    • $($probe.type): $($probe.httpGet.path)" -ForegroundColor DarkGray
            }
        }
        
        # Estado de replicas
        Write-Host "`n  📊 Estado de Replicas:" -ForegroundColor Gray
        $replicas = az containerapp replica list `
            --name $AppName `
            --resource-group $config.ResourceGroup `
            --revision $app.properties.latestRevisionName 2>$null | ConvertFrom-Json
        
        foreach ($replica in $replicas) {
            $status = $replica.properties.runningState
            $icon = if ($status -eq "Running") { "🟢" } else { "🔴" }
            Write-Host "    $icon $($replica.name): $status" -ForegroundColor DarkGray
        }
        
        return $app
    } catch {
        Write-Status "$DisplayName" "No encontrado" -IsGood $false
        return $null
    }
}

# Verificar Backend
$backend = Show-ContainerAppInfo -AppName $config.BackendContainerApp -DisplayName "Backend Container App"

# Verificar Frontend
$frontend = Show-ContainerAppInfo -AppName $config.FrontendContainerApp -DisplayName "Frontend Container App"

# Verificar conectividad
if ($backend -and $frontend) {
    Write-Host "`n🔗 Conectividad" -ForegroundColor Yellow
    
    $backendUrl = "https://$($backend.properties.configuration.ingress.fqdn)"
    $frontendUrl = "https://$($frontend.properties.configuration.ingress.fqdn)"
    
    Write-Info "Frontend -> Backend" $backendUrl
    Write-Info "Backend -> Frontend" $frontendUrl
    
    # Test de conectividad
    Write-Host "`n  🧪 Probando endpoints..." -ForegroundColor Gray
    
    try {
        $backendResponse = Invoke-WebRequest -Uri "$backendUrl/api/auth/health" -Method GET -TimeoutSec 5 2>$null
        Write-Status "Backend Health" "OK ($($backendResponse.StatusCode))" -IsGood $true
    } catch {
        Write-Status "Backend Health" "Error o endpoint no existe" -IsGood $false
    }
    
    try {
        $frontendResponse = Invoke-WebRequest -Uri $frontendUrl -Method GET -TimeoutSec 5 2>$null
        Write-Status "Frontend" "OK ($($frontendResponse.StatusCode))" -IsGood $true
    } catch {
        Write-Status "Frontend" "Error" -IsGood $false
    }
}

# Mostrar logs si se solicita
if ($ShowLogs -and $backend) {
    Write-Host "`n📜 Logs Recientes - Backend" -ForegroundColor Yellow
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkGray
    az containerapp logs show `
        --name $config.BackendContainerApp `
        --resource-group $config.ResourceGroup `
        --tail 20 `
        --output table
}

if ($ShowLogs -and $frontend) {
    Write-Host "`n📜 Logs Recientes - Frontend" -ForegroundColor Yellow
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkGray
    az containerapp logs show `
        --name $config.FrontendContainerApp `
        --resource-group $config.ResourceGroup `
        --tail 20 `
        --output table
}

# Resumen
Write-Header "Resumen"
Write-Host ""

if ($backend -and $frontend) {
    Write-Host "  ✅ Todos los servicios están desplegados y funcionando" -ForegroundColor Green
    Write-Host "`n  🌐 URLs de Acceso:" -ForegroundColor Cyan
    Write-Host "     Frontend: https://$($frontend.properties.configuration.ingress.fqdn)" -ForegroundColor White
    Write-Host "     Backend:  https://$($backend.properties.configuration.ingress.fqdn)" -ForegroundColor White
} else {
    Write-Host "  ⚠️  Algunos servicios no están disponibles" -ForegroundColor Yellow
}

Write-Host ""
