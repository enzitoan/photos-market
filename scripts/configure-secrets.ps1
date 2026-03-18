# Script para Configurar Secretos en Azure Key Vault
# PhotosMarket - Configuración Manual de Secretos

param(
    [Parameter(Mandatory=$true)]
    [string]$KeyVaultName,
    
    [Parameter(Mandatory=$false)]
    [string]$GoogleOAuthClientId,
    
    [Parameter(Mandatory=$false)]
    [string]$GoogleOAuthClientSecret,
    
    [Parameter(Mandatory=$false)]
    [string]$GoogleDriveFolderId,
    
    [Parameter(Mandatory=$false)]
    [string]$GoogleDriveCredentialsPath,
    
    [Parameter(Mandatory=$false)]
    [switch]$GenerateJwtKey
)

$ErrorActionPreference = "Stop"

Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  PHOTOSMARKET - CONFIGURACIÓN DE SECRETOS" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════`n" -ForegroundColor Cyan

# Verificar acceso a Key Vault
Write-Host "→ Verificando acceso a Key Vault: $KeyVaultName..." -ForegroundColor Yellow
$vault = az keyvault show --name $KeyVaultName 2>$null | ConvertFrom-Json
if (-not $vault) {
    Write-Host "✗ No se puede acceder a Key Vault: $KeyVaultName" -ForegroundColor Red
    Write-Host "  Asegúrate de tener permisos y que el nombre sea correcto." -ForegroundColor White
    exit 1
}
Write-Host "✓ Acceso confirmado a Key Vault`n" -ForegroundColor Green

$secretsConfigured = 0

# 1. Google OAuth Client ID
if ($GoogleOAuthClientId) {
    Write-Host "→ Configurando Google OAuth Client ID..." -ForegroundColor Yellow
    az keyvault secret set `
        --vault-name $KeyVaultName `
        --name 'GoogleOAuthClientId' `
        --value $GoogleOAuthClientId `
        --output none
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Google OAuth Client ID configurado" -ForegroundColor Green
        $secretsConfigured++
    } else {
        Write-Host "✗ Error al configurar Google OAuth Client ID" -ForegroundColor Red
    }
} else {
    Write-Host "⊘ Google OAuth Client ID no proporcionado (usa -GoogleOAuthClientId)" -ForegroundColor Gray
}
Write-Host ""

# 2. Google OAuth Client Secret
if ($GoogleOAuthClientSecret) {
    Write-Host "→ Configurando Google OAuth Client Secret..." -ForegroundColor Yellow
    az keyvault secret set `
        --vault-name $KeyVaultName `
        --name 'GoogleOAuthClientSecret' `
        --value $GoogleOAuthClientSecret `
        --output none
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Google OAuth Client Secret configurado" -ForegroundColor Green
        $secretsConfigured++
    } else {
        Write-Host "✗ Error al configurar Google OAuth Client Secret" -ForegroundColor Red
    }
} else {
    Write-Host "⊘ Google OAuth Client Secret no proporcionado (usa -GoogleOAuthClientSecret)" -ForegroundColor Gray
}
Write-Host ""

# 3. JWT Secret Key
if ($GenerateJwtKey) {
    Write-Host "→ Generando JWT Secret Key..." -ForegroundColor Yellow
    $jwtKey = -join ((48..57) + (65..90) + (97..122) | Get-Random -Count 64 | % {[char]$_})
    
    az keyvault secret set `
        --vault-name $KeyVaultName `
        --name 'JwtSecretKey' `
        --value $jwtKey `
        --output none
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ JWT Secret Key generado y configurado" -ForegroundColor Green
        Write-Host "  Longitud: 64 caracteres" -ForegroundColor Gray
        $secretsConfigured++
    } else {
        Write-Host "✗ Error al configurar JWT Secret Key" -ForegroundColor Red
    }
} else {
    Write-Host "⊘ JWT Secret Key no generado (usa -GenerateJwtKey)" -ForegroundColor Gray
}
Write-Host ""

# 4. Google Drive Folder ID
if ($GoogleDriveFolderId) {
    Write-Host "→ Configurando Google Drive Folder ID..." -ForegroundColor Yellow
    az keyvault secret set `
        --vault-name $KeyVaultName `
        --name 'GoogleDriveFolderId' `
        --value $GoogleDriveFolderId `
        --output none
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Google Drive Folder ID configurado" -ForegroundColor Green
        $secretsConfigured++
    } else {
        Write-Host "✗ Error al configurar Google Drive Folder ID" -ForegroundColor Red
    }
} else {
    Write-Host "⊘ Google Drive Folder ID no proporcionado (usa -GoogleDriveFolderId)" -ForegroundColor Gray
}
Write-Host ""

# 5. Google Drive Credentials (Service Account JSON)
if ($GoogleDriveCredentialsPath) {
    Write-Host "→ Configurando Google Drive Credentials..." -ForegroundColor Yellow
    
    if (Test-Path $GoogleDriveCredentialsPath) {
        $credentialsJson = Get-Content $GoogleDriveCredentialsPath -Raw
        
        az keyvault secret set `
            --vault-name $KeyVaultName `
            --name 'GoogleDriveCredentials' `
            --value $credentialsJson `
            --output none
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✓ Google Drive Credentials configurado" -ForegroundColor Green
            $secretsConfigured++
        } else {
            Write-Host "✗ Error al configurar Google Drive Credentials" -ForegroundColor Red
        }
    } else {
        Write-Host "✗ Archivo no encontrado: $GoogleDriveCredentialsPath" -ForegroundColor Red
    }
} else {
    Write-Host "⊘ Google Drive Credentials no proporcionado (usa -GoogleDriveCredentialsPath)" -ForegroundColor Gray
}
Write-Host ""

# Resumen
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  RESUMEN DE CONFIGURACIÓN" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════`n" -ForegroundColor Cyan

Write-Host "Secretos configurados: $secretsConfigured" -ForegroundColor $(if ($secretsConfigured -gt 0) { "Green" } else { "Yellow" })
Write-Host "Key Vault: $KeyVaultName" -ForegroundColor White
Write-Host ""

if ($secretsConfigured -eq 0) {
    Write-Host "⚠ No se configuró ningún secreto." -ForegroundColor Yellow
    Write-Host "  Usa los parámetros para proporcionar los valores:" -ForegroundColor White
    Write-Host ""
    Write-Host "  Ejemplo:" -ForegroundColor Cyan
    Write-Host "    ./configure-secrets.ps1 ``" -ForegroundColor White
    Write-Host "      -KeyVaultName '$KeyVaultName' ``" -ForegroundColor White
    Write-Host "      -GoogleOAuthClientId 'tu-client-id' ``" -ForegroundColor White
    Write-Host "      -GoogleOAuthClientSecret 'tu-client-secret' ``" -ForegroundColor White
    Write-Host "      -GenerateJwtKey ``" -ForegroundColor White
    Write-Host "      -GoogleDriveFolderId 'tu-folder-id'" -ForegroundColor White
    Write-Host ""
} else {
    Write-Host "✓ Configuración completada exitosamente" -ForegroundColor Green
    Write-Host ""
    Write-Host "Próximos pasos:" -ForegroundColor Yellow
    Write-Host "  1. Verifica los secretos en Azure Portal:" -ForegroundColor White
    Write-Host "     https://portal.azure.com/#@/resource$($vault.id)/secrets" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "  2. Reinicia las Container Apps para aplicar los cambios:" -ForegroundColor White
    Write-Host "     az containerapp revision list --name photosmarket-backend-dev -g <resource-group> --query '[0].name' -o tsv | ``" -ForegroundColor Cyan
    Write-Host "       xargs -I {} az containerapp revision restart --name photosmarket-backend-dev -g <resource-group> --revision {}" -ForegroundColor Cyan
    Write-Host ""
}

Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
