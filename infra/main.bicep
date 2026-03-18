targetScope = 'resourceGroup'

@description('Location for all resources')
param location string = resourceGroup().location

@description('Environment name (dev, staging, prod)')
@allowed([
  'dev'
  'staging'
  'prod'
])
param environmentName string = 'dev'

@description('Application name')
param appName string = 'photosmarket'

@description('CosmosDB account name')
param cosmosDbAccountName string = '${appName}-cosmos-${environmentName}'

@description('Container Registry name')
param containerRegistryName string = '${appName}acr${environmentName}'

@description('Log Analytics Workspace name')
param logAnalyticsWorkspaceName string = '${appName}-logs-${environmentName}'

@description('Container Apps Environment name')
param containerAppsEnvironmentName string = '${appName}-env-${environmentName}'

@description('Key Vault name')
param keyVaultName string = '${appName}-kv-${environmentName}'

@description('Backend container image')
param backendImage string = '${containerRegistryName}.azurecr.io/photosmarket-backend:latest'

@description('Frontend container image')
param frontendImage string = '${containerRegistryName}.azurecr.io/photosmarket-frontend:latest'

@description('Google OAuth Client ID')
@secure()
param googleOAuthClientId string

@description('Google OAuth Client Secret')
@secure()
param googleOAuthClientSecret string

@description('Google Drive Root Folder ID')
param googleDriveRootFolderId string

@description('JWT Secret Key')
@secure()
param jwtSecretKey string

// Container Registry
module containerRegistry 'modules/container-registry.bicep' = {
  name: 'containerRegistry'
  params: {
    name: containerRegistryName
    location: location
  }
}

// Log Analytics Workspace
module logAnalytics 'modules/log-analytics.bicep' = {
  name: 'logAnalytics'
  params: {
    name: logAnalyticsWorkspaceName
    location: location
  }
}

// CosmosDB
module cosmosDb 'modules/cosmos-db.bicep' = {
  name: 'cosmosDb'
  params: {
    accountName: cosmosDbAccountName
    location: location
    databaseName: 'PhotosMarketDb'
  }
}

// Key Vault
module keyVault 'modules/key-vault.bicep' = {
  name: 'keyVault'
  params: {
    name: keyVaultName
    location: location
    googleOAuthClientId: googleOAuthClientId
    googleOAuthClientSecret: googleOAuthClientSecret
    jwtSecretKey: jwtSecretKey
    cosmosDbConnectionString: cosmosDb.outputs.connectionString
  }
}

// Container Apps Environment
module containerAppsEnvironment 'modules/container-apps-environment.bicep' = {
  name: 'containerAppsEnvironment'
  params: {
    name: containerAppsEnvironmentName
    location: location
    logAnalyticsWorkspaceId: logAnalytics.outputs.workspaceId
    logAnalyticsWorkspaceKey: logAnalytics.outputs.workspaceKey
  }
}

// Backend Container App
module backendApp 'modules/backend-container-app.bicep' = {
  name: 'backendApp'
  params: {
    name: '${appName}-backend-${environmentName}'
    location: location
    containerAppsEnvironmentId: containerAppsEnvironment.outputs.id
    containerImage: backendImage
    containerRegistryName: containerRegistryName
    keyVaultUri: keyVault.outputs.uri
    googleDriveRootFolderId: googleDriveRootFolderId
    frontendUrl: 'https://${appName}-frontend-${environmentName}.${containerAppsEnvironment.outputs.defaultDomain}'
  }
  dependsOn: [
    containerRegistry
    keyVault
  ]
}

// Frontend Container App
module frontendApp 'modules/frontend-container-app.bicep' = {
  name: 'frontendApp'
  params: {
    name: '${appName}-frontend'
    location: location
    containerAppsEnvironmentId: containerAppsEnvironment.outputs.id
    containerImage: frontendImage
    containerRegistryName: containerRegistryName
    backendUrl: backendApp.outputs.fqdn
  }
  dependsOn: [
    containerRegistry
  ]
}

// Role Assignments for Backend Container App

// Existing Key Vault resource reference for role assignment
resource existingKeyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

// Existing Container Registry resource reference for role assignment
resource existingContainerRegistry 'Microsoft.ContainerRegistry/registries@2023-07-01' existing = {
  name: containerRegistryName
}

// Assign Key Vault Secrets User role to Backend Container App
resource backendKeyVaultRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(existingKeyVault.id, '${appName}-backend', 'Key Vault Secrets User')
  scope: existingKeyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '4633458b-17de-408a-b874-0445c86b69e6') // Key Vault Secrets User
    principalId: backendApp.outputs.principalId
    principalType: 'ServicePrincipal'
  }
}

// Assign ACR Pull role to Backend Container App
resource backendAcrPullRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(existingContainerRegistry.id, '${appName}-backend', 'AcrPull')
  scope: existingContainerRegistry
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d') // AcrPull
    principalId: backendApp.outputs.principalId
    principalType: 'ServicePrincipal'
  }
}

// Assign ACR Pull role to Frontend Container App
resource frontendAcrPullRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(existingContainerRegistry.id, '${appName}-frontend', 'AcrPull')
  scope: existingContainerRegistry
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d') // AcrPull
    principalId: frontendApp.outputs.principalId
    principalType: 'ServicePrincipal'
  }
}

// Outputs
output backendUrl string = 'https://${backendApp.outputs.fqdn}'
output frontendUrl string = 'https://${frontendApp.outputs.fqdn}'
output containerRegistryLoginServer string = containerRegistry.outputs.loginServer
output cosmosDbEndpoint string = cosmosDb.outputs.endpoint
output keyVaultUri string = keyVault.outputs.uri
