using './main.bicep'

param environmentName = 'dev'
param appName = 'photosmarket'
param location = 'eastus'
param googleDriveRootFolderId = '1JoezTDvrHG76ICArjBhFBdZEOW8tWn05'

// Secrets - These should be provided during deployment
param googleOAuthClientId = readEnvironmentVariable('GOOGLE_OAUTH_CLIENT_ID', '')
param googleOAuthClientSecret = readEnvironmentVariable('GOOGLE_OAUTH_CLIENT_SECRET', '')
param jwtSecretKey = readEnvironmentVariable('JWT_SECRET_KEY', '')
