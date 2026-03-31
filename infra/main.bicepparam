using './main.bicep'

param environmentName = 'dev'
param appName = 'photosmarket'
param location = 'eastus'
param googleDriveRootFolderId = '1JoezTDvrHG76ICArjBhFBdZEOW8tWn05'

// Secrets - These should be provided during deployment
param googleOAuthClientId = readEnvironmentVariable('GOOGLE_OAUTH_CLIENT_ID', '')
param googleOAuthClientSecret = readEnvironmentVariable('GOOGLE_OAUTH_CLIENT_SECRET', '')
param jwtSecretKey = readEnvironmentVariable('JWT_SECRET_KEY', '')
param googleDriveCredentials = readEnvironmentVariable('GOOGLE_DRIVE_CREDENTIALS', '')
param emailApiKey = readEnvironmentVariable('EMAIL_API_KEY', '')

// Optional - Email configuration
param emailEnabled = true

// Optional - Watermark configuration (can be customized per deployment)
param watermarkFontSizeDivisor = '55'     // Marca de tamaño balanceado
param watermarkTextOpacity = '0.6'        // Opacidad moderada (visible pero no intrusiva)
param watermarkShadowOpacity = '0.3'      // Sombra sutil para contraste
param watermarkVerticalPosition = '0.9'   // Posición inferior
