# Local Development Script for PhotosMarket with Docker
param(
    [Parameter(Mandatory=$false)]
    [ValidateSet('start', 'stop', 'restart', 'logs', 'build', 'clean')]
    [string]$Action = 'start'
)

$ErrorActionPreference = "Stop"

function Write-Header {
    param([string]$Text)
    Write-Host "`n═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "  $Text" -ForegroundColor Cyan
    Write-Host "═══════════════════════════════════════════════════════════`n" -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Text)
    Write-Host "✓ $Text" -ForegroundColor Green
}

function Write-Info {
    param([string]$Text)
    Write-Host "→ $Text" -ForegroundColor Yellow
}

function Write-Error-Message {
    param([string]$Text)
    Write-Host "✗ $Text" -ForegroundColor Red
}

# Check Docker
Write-Info "Verificando Docker..."
docker info > $null 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Error-Message "Docker no está corriendo"
    Write-Host "Por favor, inicia Docker Desktop e intenta nuevamente" -ForegroundColor White
    exit 1
}
Write-Success "Docker está corriendo`n"

# Check .env file
if (-not (Test-Path ".env")) {
    Write-Info "Archivo .env no encontrado, creando desde .env.example..."
    if (Test-Path ".env.example") {
        Copy-Item ".env.example" ".env"
        Write-Success "Archivo .env creado"
        Write-Host "`n⚠️  IMPORTANTE: Edita el archivo .env con tus credenciales antes de continuar`n" -ForegroundColor Yellow
        exit 0
    } else {
        Write-Error-Message "Archivo .env.example no encontrado"
        exit 1
    }
}

switch ($Action) {
    'start' {
        Write-Header "INICIANDO PHOTOSMARKET EN DOCKER"
        
        Write-Info "Iniciando servicios..."
        docker-compose up -d
        
        if ($LASTEXITCODE -eq 0) {
            Start-Sleep -Seconds 3
            Write-Success "Servicios iniciados correctamente`n"
            
            Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Green
            Write-Host "  ✓ PHOTOSMARKET CORRIENDO" -ForegroundColor Green
            Write-Host "═══════════════════════════════════════════════════════════`n" -ForegroundColor Green
            
            Write-Host "URLS:" -ForegroundColor Cyan
            Write-Host "  Frontend:   http://localhost:3001" -ForegroundColor White
            Write-Host "  Backend:    http://localhost:5000" -ForegroundColor White
            Write-Host "  Swagger:    http://localhost:5000/swagger" -ForegroundColor White
            Write-Host ""
            
            Write-Host "COMANDOS ÚTILES:" -ForegroundColor Cyan
            Write-Host "  Ver logs:     .\docker-dev.ps1 -Action logs" -ForegroundColor White
            Write-Host "  Reiniciar:    .\docker-dev.ps1 -Action restart" -ForegroundColor White
            Write-Host "  Detener:      .\docker-dev.ps1 -Action stop" -ForegroundColor White
            Write-Host ""
        } else {
            Write-Error-Message "Error al iniciar servicios"
            docker-compose logs
            exit 1
        }
    }
    
    'stop' {
        Write-Header "DETENIENDO SERVICIOS"
        
        Write-Info "Deteniendo contenedores..."
        docker-compose down
        
        if ($LASTEXITCODE -eq 0) {
            Write-Success "Servicios detenidos correctamente"
        } else {
            Write-Error-Message "Error al detener servicios"
            exit 1
        }
    }
    
    'restart' {
        Write-Header "REINICIANDO SERVICIOS"
        
        Write-Info "Deteniendo servicios..."
        docker-compose down
        
        Write-Info "Iniciando servicios..."
        docker-compose up -d
        
        if ($LASTEXITCODE -eq 0) {
            Start-Sleep -Seconds 3
            Write-Success "Servicios reiniciados correctamente"
            Write-Host "`n  Frontend:   http://localhost:3001" -ForegroundColor Cyan
            Write-Host "  Backend:    http://localhost:5000" -ForegroundColor Cyan
        } else {
            Write-Error-Message "Error al reiniciar servicios"
            exit 1
        }
    }
    
    'logs' {
        Write-Header "LOGS DE LOS SERVICIOS"
        docker-compose logs -f --tail=100
    }
    
    'build' {
        Write-Header "CONSTRUYENDO IMÁGENES"
        
        Write-Info "Construyendo backend..."
        docker-compose build backend
        
        if ($LASTEXITCODE -ne 0) {
            Write-Error-Message "Error al construir backend"
            exit 1
        }
        Write-Success "Backend construido"
        
        Write-Info "Construyendo frontend..."
        docker-compose build frontend
        
        if ($LASTEXITCODE -ne 0) {
            Write-Error-Message "Error al construir frontend"
            exit 1
        }
        Write-Success "Frontend construido"
        
        Write-Host "`n✓ Todas las imágenes construidas correctamente" -ForegroundColor Green
        Write-Host "  Usa: .\docker-dev.ps1 -Action start" -ForegroundColor White
    }
    
    'clean' {
        Write-Header "LIMPIANDO RECURSOS DOCKER"
        
        Write-Info "Deteniendo servicios..."
        docker-compose down -v
        
        Write-Info "Eliminando imágenes..."
        docker-compose down --rmi local
        
        Write-Info "Limpiando recursos no usados..."
        docker system prune -f
        
        Write-Success "Limpieza completada"
    }
}
