# Docker Build Script for PhotosMarket
param(
    [Parameter(Mandatory=$false)]
    [switch]$Push,

    [Parameter(Mandatory=$false)]
    [string]$Registry = "photosmarketacrdev",

    [Parameter(Mandatory=$false)]
    [string]$Tag = "latest"
)

$ErrorActionPreference = "Stop"

Write-Host "`n═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  PHOTOSMARKET - DOCKER BUILD SCRIPT" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════`n" -ForegroundColor Cyan

# Check if Docker is running
Write-Host "→ Verificando Docker..." -ForegroundColor Yellow
docker info > $null 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Docker no está corriendo o no está instalado" -ForegroundColor Red
    Write-Host "  Inicia Docker Desktop e intenta nuevamente" -ForegroundColor White
    exit 1
}
Write-Host "✓ Docker está corriendo`n" -ForegroundColor Green

# Set image names
$backendImage = if ($Registry) { "${Registry}.azurecr.io/photosmarket-backend:${Tag}" } else { "photosmarket-backend:${Tag}" }
$frontendImage = if ($Registry) { "${Registry}.azurecr.io/photosmarket-frontend:${Tag}" } else { "photosmarket-frontend:${Tag}" }

# Build backend
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  CONSTRUYENDO BACKEND" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════`n" -ForegroundColor Cyan

Write-Host "→ Imagen: $backendImage" -ForegroundColor White
docker build -t $backendImage -f ../src/backend/Dockerfile ../src/backend
if ($LASTEXITCODE -ne 0) {
    Write-Host "`n✗ Error al construir imagen del backend" -ForegroundColor Red
    exit 1
}
Write-Host "`n✓ Backend construido exitosamente`n" -ForegroundColor Green

# Build frontend
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  CONSTRUYENDO FRONTEND" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════`n" -ForegroundColor Cyan

Write-Host "→ Imagen: $frontendImage" -ForegroundColor White
docker build -t $frontendImage -f ../src/frontend/Dockerfile ../src/frontend
if ($LASTEXITCODE -ne 0) {
    Write-Host "`n✗ Error al construir imagen del frontend" -ForegroundColor Red
    exit 1
}
Write-Host "`n✓ Frontend construido exitosamente`n" -ForegroundColor Green

# Push images if requested
if ($Push) {
    if (-not $Registry) {
        Write-Host "✗ No se puede hacer push sin especificar un registry" -ForegroundColor Red
        Write-Host "  Usa -Registry para especificar el nombre del Azure Container Registry" -ForegroundColor White
        exit 1
    }

    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "  PUBLICANDO IMÁGENES" -ForegroundColor Cyan
    Write-Host "═══════════════════════════════════════════════════════════`n" -ForegroundColor Cyan

    # Login to ACR
    Write-Host "→ Iniciando sesión en Azure Container Registry..." -ForegroundColor Yellow
    az acr login --name $Registry
    if ($LASTEXITCODE -ne 0) {
        Write-Host "✗ Error al iniciar sesión en ACR" -ForegroundColor Red
        Write-Host "  Asegúrate de estar autenticado con Azure CLI" -ForegroundColor White
        exit 1
    }
    Write-Host "✓ Sesión iniciada`n" -ForegroundColor Green

    # Push backend
    Write-Host "→ Publicando backend..." -ForegroundColor Yellow
    docker push $backendImage
    if ($LASTEXITCODE -ne 0) {
        Write-Host "✗ Error al publicar imagen del backend" -ForegroundColor Red
        exit 1
    }
    Write-Host "✓ Backend publicado`n" -ForegroundColor Green

    # Push frontend
    Write-Host "→ Publicando frontend..." -ForegroundColor Yellow
    docker push $frontendImage
    if ($LASTEXITCODE -ne 0) {
        Write-Host "✗ Error al publicar imagen del frontend" -ForegroundColor Red
        exit 1
    }
    Write-Host "✓ Frontend publicado`n" -ForegroundColor Green
}

# Summary
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host "  ✓ BUILD COMPLETADO" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════════`n" -ForegroundColor Green

Write-Host "IMÁGENES CONSTRUIDAS:" -ForegroundColor Cyan
Write-Host "  Backend:  $backendImage" -ForegroundColor White
Write-Host "  Frontend: $frontendImage" -ForegroundColor White
Write-Host ""

if (-not $Push) {
    Write-Host "Para probar localmente, ejecuta:" -ForegroundColor Yellow
    Write-Host "  docker-compose up" -ForegroundColor White
    Write-Host ""
    Write-Host "Para publicar a Azure, ejecuta:" -ForegroundColor Yellow
    Write-Host "  .\build-docker.ps1 -Push -Registry <nombre-acr>" -ForegroundColor White
    Write-Host ""
}
