#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Inicia los proyectos Backend y Frontend de PhotosMarket en modo desarrollo.

.DESCRIPTION
    Este script inicia el backend (.NET) y el frontend (Vue.js) de PhotosMarket
    en procesos separados en segundo plano.

.EXAMPLE
    .\Start-PhotosMarket.ps1
    Inicia ambos proyectos

.EXAMPLE
    .\Start-PhotosMarket.ps1 -BackendOnly
    Inicia solo el backend

.EXAMPLE
    .\Start-PhotosMarket.ps1 -FrontendOnly
    Inicia solo el frontend
#>

param(
    [switch]$BackendOnly,
    [switch]$FrontendOnly
)

# Colores para output
function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    Write-Host $Message -ForegroundColor $Color
}

# Verificar que estamos en la raíz del proyecto
$rootPath = $PSScriptRoot
if (-not (Test-Path "$rootPath\src\backend") -or -not (Test-Path "$rootPath\src\frontend")) {
    Write-ColorOutput "❌ Error: Este script debe ejecutarse desde la raíz del proyecto PhotosMarket" "Red"
    exit 1
}

Write-ColorOutput "`n🚀 PhotosMarket - Iniciando Servicios`n" "Cyan"

# Función para iniciar el backend
function Start-Backend {
    Write-ColorOutput "📦 Iniciando Backend (.NET)..." "Yellow"
    
    $backendPath = Join-Path $rootPath "src\backend"
    
    # Verificar si ya está corriendo
    $existingBackend = Get-Process | Where-Object { $_.ProcessName -like "*PhotosMarket.API*" }
    if ($existingBackend) {
        Write-ColorOutput "⚠️  Backend ya está en ejecución (PID: $($existingBackend.Id))" "Yellow"
        return
    }
    
    # Iniciar backend
    $backendJob = Start-Process pwsh -ArgumentList "-NoExit", "-Command", "cd '$backendPath'; Write-Host '🔧 Backend iniciando en http://localhost:5001' -ForegroundColor Green; dotnet run" -PassThru
    
    Write-ColorOutput "✅ Backend iniciado (PID: $($backendJob.Id))" "Green"
    Write-ColorOutput "   URL: http://localhost:5001" "Gray"
    Write-ColorOutput "   Swagger: http://localhost:5001/swagger" "Gray"
}

# Función para iniciar el frontend
function Start-Frontend {
    Write-ColorOutput "`n📦 Iniciando Frontend (Vue.js)..." "Yellow"
    
    $frontendPath = Join-Path $rootPath "src\frontend"
    
    # Verificar si ya está corriendo (buscando proceso node en puerto 3001)
    $port = 3001
    $existingProcess = Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue
    if ($existingProcess) {
        Write-ColorOutput "⚠️  Frontend ya está en ejecución en puerto $port" "Yellow"
        return
    }
    
    # Verificar que node_modules existe
    if (-not (Test-Path "$frontendPath\node_modules")) {
        Write-ColorOutput "📥 Instalando dependencias del frontend..." "Yellow"
        Push-Location $frontendPath
        npm install
        Pop-Location
    }
    
    # Iniciar frontend
    $frontendJob = Start-Process pwsh -ArgumentList "-NoExit", "-Command", "cd '$frontendPath'; Write-Host '🎨 Frontend iniciando en http://localhost:3001' -ForegroundColor Cyan; npm run dev" -PassThru
    
    Write-ColorOutput "✅ Frontend iniciado (PID: $($frontendJob.Id))" "Green"
    Write-ColorOutput "   URL: http://localhost:3001" "Gray"
}

# Ejecutar según parámetros
if ($FrontendOnly) {
    Start-Frontend
}
elseif ($BackendOnly) {
    Start-Backend
}
else {
    # Iniciar ambos
    Start-Backend
    Start-Sleep -Seconds 2
    Start-Frontend
}

Write-ColorOutput "`n✨ Servicios iniciados correctamente" "Green"
Write-ColorOutput "`n📌 URLs Disponibles:" "Cyan"
if (-not $FrontendOnly) {
    Write-ColorOutput "   Backend API:  http://localhost:5001" "White"
    Write-ColorOutput "   Swagger UI:   http://localhost:5001/swagger" "White"
}
if (-not $BackendOnly) {
    Write-ColorOutput "   Frontend App: http://localhost:3001" "White"
}

Write-ColorOutput "`n💡 Para detener los servicios, ejecuta: .\Stop-PhotosMarket.ps1" "Yellow"
Write-ColorOutput "💡 Para ver logs, revisa las ventanas abiertas`n" "Yellow"
