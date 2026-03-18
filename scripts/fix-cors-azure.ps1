# Script para aplicar la corrección de CORS inmediatamente
# Este script actualiza las container apps existentes con las configuraciones correctas

Write-Host "`n╔═══════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║       APLICANDO CORRECCIÓN DE CORS - AZURE          ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════╝`n" -ForegroundColor Cyan

$RESOURCE_GROUP = "rg-photosmarket-dev"
$BACKEND_APP = "photosmarket-backend-dev"
$FRONTEND_APP = "photosmarket-frontend-dev"

# Obtener las URLs actuales
Write-Host "→ Obteniendo URLs de las Container Apps..." -ForegroundColor White

$BACKEND_URL = az containerapp show `
  --name $BACKEND_APP `
  --resource-group $RESOURCE_GROUP `
  --query "properties.configuration.ingress.fqdn" -o tsv

$FRONTEND_URL = az containerapp show `
  --name $FRONTEND_APP `
  --resource-group $RESOURCE_GROUP `
  --query "properties.configuration.ingress.fqdn" -o tsv

Write-Host "✓ Backend URL: https://$BACKEND_URL" -ForegroundColor Green
Write-Host "✓ Frontend URL: https://$FRONTEND_URL" -ForegroundColor Green

# Actualizar Backend con la URL del Frontend para CORS
Write-Host "`n→ Actualizando Backend con CORS para Frontend..." -ForegroundColor White

az containerapp update `
  --name $BACKEND_APP `
  --resource-group $RESOURCE_GROUP `
  --set-env-vars "FRONTEND_URL=https://$FRONTEND_URL"

if ($LASTEXITCODE -eq 0) {
  Write-Host "✓ Backend actualizado correctamente" -ForegroundColor Green
} else {
  Write-Host "✗ Error actualizando Backend" -ForegroundColor Red
  exit 1
}

# Actualizar Frontend con la URL del Backend
Write-Host "`n→ Actualizando Frontend con URL del Backend..." -ForegroundColor White

az containerapp update `
  --name $FRONTEND_APP `
  --resource-group $RESOURCE_GROUP `
  --set-env-vars "VITE_API_URL=https://$BACKEND_URL"

if ($LASTEXITCODE -eq 0) {
  Write-Host "✓ Frontend actualizado correctamente" -ForegroundColor Green
} else {
  Write-Host "✗ Error actualizando Frontend" -ForegroundColor Red
  exit 1
}

Write-Host "`n╔════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║   ✓ CORRECCIÓN DE CORS APLICADA EXITOSAMENTE        ║" -ForegroundColor Green
Write-Host "╚════════════════════════════════════════════════════════╝`n" -ForegroundColor Green

Write-Host "URLS DE LA APLICACIÓN:" -ForegroundColor Cyan
Write-Host "  Backend:  https://$BACKEND_URL" -ForegroundColor White
Write-Host "  Frontend: https://$FRONTEND_URL`n" -ForegroundColor White

Write-Host "⚠  Las Container Apps se están reiniciando..." -ForegroundColor Yellow
Write-Host "   Espera 1-2 minutos para que los cambios surtan efecto.`n" -ForegroundColor Yellow

Write-Host "PARA VERIFICAR:" -ForegroundColor Cyan
Write-Host "  1. Abre el frontend: https://$FRONTEND_URL" -ForegroundColor White
Write-Host "  2. Intenta iniciar sesión con Google" -ForegroundColor White
Write-Host "  3. Verifica que NO haya error de CORS`n" -ForegroundColor White
