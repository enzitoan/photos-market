@description('Key Vault name')
param name string

@description('Location for the Key Vault')
param location string

@description('Google OAuth Client ID')
@secure()
param googleOAuthClientId string

@description('Google OAuth Client Secret')
@secure()
param googleOAuthClientSecret string

@description('JWT Secret Key')
@secure()
param jwtSecretKey string

@description('CosmosDB Connection String')
@secure()
param cosmosDbConnectionString string

@description('Google Drive Service Account Credentials JSON')
@secure()
param googleDriveCredentials string

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: name
  location: location
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: subscription().tenantId
    enableRbacAuthorization: true
    enabledForDeployment: false
    enabledForDiskEncryption: false
    enabledForTemplateDeployment: true
    publicNetworkAccess: 'Enabled'
  }
}

resource googleOAuthClientIdSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'GoogleOAuthClientId'
  properties: {
    value: googleOAuthClientId
  }
}

resource googleOAuthClientSecretSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'GoogleOAuthClientSecret'
  properties: {
    value: googleOAuthClientSecret
  }
}

resource jwtSecretKeySecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'JwtSecretKey'
  properties: {
    value: jwtSecretKey
  }
}

resource cosmosDbConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'CosmosDbConnectionString'
  properties: {
    value: cosmosDbConnectionString
  }
}

resource googleDriveCredentialsSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'GoogleDriveCredentials'
  properties: {
    value: googleDriveCredentials
  }
}

output id string = keyVault.id
output name string = keyVault.name
output uri string = keyVault.properties.vaultUri
