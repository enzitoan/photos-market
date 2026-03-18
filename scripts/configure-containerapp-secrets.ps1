#!/usr/bin/env pwsh
# Script para configurar secrets del Key Vault en el Container App
# Este script extrae los valores del Key Vault y los configura directamente en el Container App

param(
    [string]$ResourceGroup = "rg-photosmarket-dev",
    [string]$ContainerAppName = "photosmarket-backend-dev",
    [string]$KeyVaultName = "photosmarket-kv-dev"
)

Write-Host "`n→ Configurando secrets del Container App desde Key Vault...`n" -ForegroundColor Cyan

# Extraer valores del Key Vault
Write-Host "1. Extrayendo secrets del Key Vault..." -ForegroundColor Yellow
$cosmosConn = az keyvault secret show --vault-name $KeyVaultName --name CosmosDbConnectionString --query "value" -o tsv
$googleClientId = az keyvault secret show --vault-name $KeyVaultName --name GoogleOAuthClientId --query "value" -o tsv
$googleClientSecret = az keyvault secret show --vault-name $KeyVaultName --name GoogleOAuthClientSecret --query "value" -o tsv
$jwtSecret = az keyvault secret show --vault-name $KeyVaultName --name JwtSecretKey --query "value" -o tsv

Write-Host "   ✓ Todos los secrets extraídos exitosamente`n" -ForegroundColor Green

# Verificar que los valores no estén vacíos
if ([string]::IsNullOrWhiteSpace($cosmosConn)) {
    Write-Host "   ✗ Error: CosmosDB connection string está vacío" -ForegroundColor Red
    exit 1
}
if ([string]::IsNullOrWhiteSpace($googleClientId)) {
    Write-Host "   ✗ Error: Google Client ID está vacío" -ForegroundColor Red
    exit 1
}
if ([string]::IsNullOrWhiteSpace($googleClientSecret)) {
    Write-Host "   ✗ Error: Google Client Secret está vacío" -ForegroundColor Red
    exit 1
}
if ([string]::IsNullOrWhiteSpace($jwtSecret)) {
    Write-Host "   ✗ Error: JWT Secret está vacío" -ForegroundColor Red
    exit 1
}

# Configurar secrets en el Container App
Write-Host "2. Configurando secrets en el Container App..." -ForegroundColor Yellow

# Usar un archivo temporal para pasar los secrets de forma segura
$secretsJson = @(
    @{
        name = "cosmos-connection"
        value = $cosmosConn
    },
    @{
        name = "google-client-id"
        value = $googleClientId
    },
    @{
        name = "google-client-secret"
        value = $googleClientSecret
    },
    @{
        name = "jwt-secret"
        value = $jwtSecret
    }
) | ConvertTo-Json -Compress

Write-Host "   Secrets a configurar: cosmos-connection, google-client-id, google-client-secret, jwt-secret" -ForegroundColor Gray

# Configurar cada secret individualmente
Write-Host "`n   Configurando cosmos-connection..." -ForegroundColor Gray
az containerapp secret set --name $ContainerAppName --resource-group $ResourceGroup --secrets "cosmos-connection=$cosmosConn"

Write-Host "   Configurando google-client-id..." -ForegroundColor Gray
az containerapp secret set --name $ContainerAppName --resource-group $ResourceGroup --secrets "google-client-id=$googleClientId"

Write-Host "   Configurando google-client-secret..." -ForegroundColor Gray
az containerapp secret set --name $ContainerAppName --resource-group $ResourceGroup --secrets "google-client-secret=$googleClientSecret"

Write-Host "   Configurando jwt-secret..." -ForegroundColor Gray
az containerapp secret set --name $ContainerAppName --resource-group $ResourceGroup --secrets "jwt-secret=$jwtSecret"

Write-Host "`n   ✓ Todos los secrets configurados exitosamente`n" -ForegroundColor Green

# Actualizar variables de entorno para usar los secrets
Write-Host "3. Actualizando variables de entorno para referenciar los secrets..." -ForegroundColor Yellow
az containerapp update `
  --name $ContainerAppName `
  --resource-group $ResourceGroup `
  --set-env-vars `
    "ConnectionStrings__CosmosDb=secretref:cosmos-connection" `
    "GoogleOAuth__ClientId=secretref:google-client-id" `
    "GoogleOAuth__ClientSecret=secretref:google-client-secret" `
    "Jwt__SecretKey=secretref:jwt-secret"

Write-Host "`n✓ Configuración completada exitosamente!`n" -ForegroundColor Green
Write-Host "El backend ahora debería conectarse a Cosmos DB y tener OAuth configurado.`n" -ForegroundColor Cyan
