<#
.SYNOPSIS
    Script para ejecutar pruebas unitarias del proyecto Photos Market

.DESCRIPTION
    Este script ejecuta las pruebas unitarias del Backend (xUnit) y Frontend (Vitest).
    Es útil para ejecutar las pruebas localmente antes de hacer commits o despliegues.

.PARAMETER Component
    Componente a probar: 'Backend', 'Frontend', o 'Both' (default: 'Both')

.PARAMETER Coverage
    Si se especifica, genera reporte de cobertura de código

.EXAMPLE
    .\Run-Tests.ps1
    Ejecuta todas las pruebas (Backend y Frontend)

.EXAMPLE
    .\Run-Tests.ps1 -Component Backend
    Ejecuta solo las pruebas del Backend

.EXAMPLE
    .\Run-Tests.ps1 -Coverage
    Ejecuta todas las pruebas y genera reporte de cobertura

.NOTES
    Asegúrate de tener instalados:
    - .NET SDK 8.0+ para las pruebas del Backend
    - Node.js 18+ y npm para las pruebas del Frontend
#>

[CmdletBinding()]
param(
    [Parameter()]
    [ValidateSet('Backend', 'Frontend', 'Both')]
    [string]$Component = 'Both',

    [Parameter()]
    [switch]$Coverage
)

# Colores para output
function Write-Info { param($Message) Write-Host "ℹ️  $Message" -ForegroundColor Cyan }
function Write-Success { param($Message) Write-Host "✅ $Message" -ForegroundColor Green }
function Write-Error { param($Message) Write-Host "❌ $Message" -ForegroundColor Red }
function Write-Step { param($Message) Write-Host "`n🔄 $Message" -ForegroundColor Yellow }

# Función para ejecutar pruebas unitarias del Backend
function Test-Backend {
    param(
        [string]$ProjectRoot,
        [bool]$WithCoverage
    )
    
    Write-Step "Ejecutando pruebas unitarias del Backend..."
    
    $testProject = Join-Path $ProjectRoot "src\backend\PhotosMarket.API.Tests\PhotosMarket.API.Tests.csproj"
    
    if (-not (Test-Path $testProject)) {
        Write-Error "Proyecto de pruebas no encontrado: $testProject"
        return $false
    }
    
    Write-Info "Proyecto de pruebas: $testProject"
    
    if ($WithCoverage) {
        Write-Info "Ejecutando con reporte de cobertura..."
        dotnet test $testProject `
            --verbosity minimal `
            --logger "console;verbosity=normal" `
            --collect:"XPlat Code Coverage" `
            --results-directory "./TestResults"
    }
    else {
        dotnet test $testProject --verbosity minimal --logger "console;verbosity=normal"
    }
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Las pruebas del Backend fallaron"
        return $false
    }
    
    Write-Success "Todas las pruebas del Backend pasaron exitosamente!"
    
    if ($WithCoverage) {
        Write-Info "Reporte de cobertura generado en: ./TestResults"
    }
    
    return $true
}

# Función para ejecutar pruebas unitarias del Frontend
function Test-Frontend {
    param(
        [string]$ProjectRoot,
        [bool]$WithCoverage
    )
    
    Write-Step "Ejecutando pruebas unitarias del Frontend..."
    
    $frontendPath = Join-Path $ProjectRoot "src\frontend"
    
    if (-not (Test-Path $frontendPath)) {
        Write-Error "Frontend no encontrado: $frontendPath"
        return $false
    }
    
    Push-Location $frontendPath
    
    try {
        # Verificar que node_modules existe
        if (-not (Test-Path "node_modules")) {
            Write-Info "Instalando dependencias con npm..."
            npm install
            if ($LASTEXITCODE -ne 0) {
                Write-Error "Error al instalar dependencias"
                return $false
            }
        }
        
        # Ejecutar pruebas
        if ($WithCoverage) {
            Write-Info "Ejecutando con reporte de cobertura..."
            npm run test:coverage
        }
        else {
            Write-Info "Ejecutando pruebas con npm test..."
            npm test
        }
        
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Las pruebas del Frontend fallaron"
            return $false
        }
        
        Write-Success "Todas las pruebas del Frontend pasaron exitosamente!"
        
        if ($WithCoverage) {
            Write-Info "Reporte de cobertura generado en: src/frontend/coverage"
        }
        
        return $true
    }
    finally {
        Pop-Location
    }
}

# ====================
# SCRIPT PRINCIPAL
# ====================

Write-Host "`n╔════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║       Photos Market - Unit Tests Runner                   ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan

Write-Info "Component: $Component"
Write-Info "Coverage: $Coverage"

# Obtener directorio raíz del proyecto
$scriptPath = Split-Path -Parent $PSCommandPath
$projectRoot = Split-Path -Parent $scriptPath

Write-Info "Project Root: $projectRoot"

$allTestsPassed = $true

# Ejecutar pruebas de Backend
if ($Component -eq 'Backend' -or $Component -eq 'Both') {
    if (-not (Test-Backend -ProjectRoot $projectRoot -WithCoverage $Coverage)) {
        $allTestsPassed = $false
    }
}

# Ejecutar pruebas de Frontend
if ($Component -eq 'Frontend' -or $Component -eq 'Both') {
    if (-not (Test-Frontend -ProjectRoot $projectRoot -WithCoverage $Coverage)) {
        $allTestsPassed = $false
    }
}

# Resultado final
Write-Host ""
if ($allTestsPassed) {
    Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Green
    Write-Host "║      ✅ Todas las Pruebas Pasaron Exitosamente! ✅         ║" -ForegroundColor Green
    Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Green
    exit 0
}
else {
    Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Red
    Write-Host "║       ❌ Algunas Pruebas Fallaron ❌                       ║" -ForegroundColor Red
    Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Red
    exit 1
}
