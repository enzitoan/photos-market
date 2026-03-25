#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Detiene los proyectos Backend y Frontend de PhotosMarket.

.DESCRIPTION
    Este script detiene todos los procesos relacionados con el backend (.NET) 
    y el frontend (Vue.js/Node) de PhotosMarket.

.EXAMPLE
    .\Stop-PhotosMarket.ps1
    Detiene ambos proyectos

.EXAMPLE
    .\Stop-PhotosMarket.ps1 -BackendOnly
    Detiene solo el backend

.EXAMPLE
    .\Stop-PhotosMarket.ps1 -FrontendOnly
    Detiene solo el frontend
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

Write-ColorOutput "`n🛑 PhotosMarket - Deteniendo Servicios`n" "Cyan"

$stoppedAny = $false

# Función para detener el backend
function Stop-Backend {
    Write-ColorOutput "🔧 Buscando procesos del Backend..." "Yellow"
    
    # Buscar procesos de .NET que ejecutan PhotosMarket.API
    $backendProcesses = Get-Process | Where-Object { 
        $_.ProcessName -like "*PhotosMarket.API*" -or 
        ($_.ProcessName -eq "dotnet" -and $_.Path -like "*PhotosMarket*")
    }
    
    if ($backendProcesses) {
        foreach ($process in $backendProcesses) {
            try {
                Write-ColorOutput "   Deteniendo proceso: $($process.ProcessName) (PID: $($process.Id))" "Gray"
                Stop-Process -Id $process.Id -Force -ErrorAction Stop
                $script:stoppedAny = $true
            }
            catch {
                Write-ColorOutput "   ⚠️  No se pudo detener el proceso $($process.Id): $_" "Yellow"
            }
        }
        Write-ColorOutput "✅ Backend detenido" "Green"
    }
    else {
        Write-ColorOutput "ℹ️  No se encontraron procesos del backend en ejecución" "Gray"
    }
}

# Función para detener el frontend
function Stop-Frontend {
    Write-ColorOutput "`n🎨 Buscando procesos del Frontend..." "Yellow"
    
    # Buscar procesos de Node/npm en puerto 3001 o relacionados con Vite
    $frontendProcesses = @()
    
    # Buscar por puerto
    $port = 3001
    $connection = Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue
    if ($connection) {
        $processId = $connection.OwningProcess
        $process = Get-Process -Id $processId -ErrorAction SilentlyContinue
        if ($process) {
            $frontendProcesses += $process
        }
    }
    
    # Buscar procesos de node relacionados con vite o vue
    $nodeProcesses = Get-Process -Name "node" -ErrorAction SilentlyContinue | Where-Object {
        $cmdLine = (Get-CimInstance Win32_Process -Filter "ProcessId = $($_.Id)" -ErrorAction SilentlyContinue).CommandLine
        $cmdLine -like "*vite*" -or $cmdLine -like "*vue*" -or $cmdLine -like "*frontend*"
    }
    
    $frontendProcesses += $nodeProcesses
    
    # Eliminar duplicados
    $frontendProcesses = $frontendProcesses | Select-Object -Unique
    
    if ($frontendProcesses) {
        foreach ($process in $frontendProcesses) {
            try {
                Write-ColorOutput "   Deteniendo proceso: $($process.ProcessName) (PID: $($process.Id))" "Gray"
                Stop-Process -Id $process.Id -Force -ErrorAction Stop
                $script:stoppedAny = $true
            }
            catch {
                Write-ColorOutput "   ⚠️  No se pudo detener el proceso $($process.Id): $_" "Yellow"
            }
        }
        Write-ColorOutput "✅ Frontend detenido" "Green"
    }
    else {
        Write-ColorOutput "ℹ️  No se encontraron procesos del frontend en ejecución" "Gray"
    }
}

# Ejecutar según parámetros
if ($FrontendOnly) {
    Stop-Frontend
}
elseif ($BackendOnly) {
    Stop-Backend
}
else {
    # Detener ambos
    Stop-Backend
    Stop-Frontend
}

# Cerrar ventanas de PowerShell adicionales si las hay
Write-ColorOutput "`n🧹 Limpiando ventanas de terminal adicionales..." "Yellow"
$currentPid = $PID
Get-Process pwsh -ErrorAction SilentlyContinue | Where-Object { 
    $_.Id -ne $currentPid -and 
    $_.MainWindowTitle -like "*PhotosMarket*"
} | ForEach-Object {
    try {
        Write-ColorOutput "   Cerrando ventana: $($_.MainWindowTitle)" "Gray"
        Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue
    }
    catch {
        # Ignorar errores
    }
}

if ($stoppedAny) {
    Write-ColorOutput "`n✅ Servicios detenidos correctamente`n" "Green"
}
else {
    Write-ColorOutput "`nℹ️  No había servicios en ejecución`n" "Gray"
}

Write-ColorOutput "💡 Para iniciar los servicios nuevamente, ejecuta: .\Start-PhotosMarket.ps1`n" "Yellow"
