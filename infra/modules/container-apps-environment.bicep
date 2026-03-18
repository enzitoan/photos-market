@description('Container Apps Environment name')
param name string

@description('Location for the Container Apps Environment')
param location string

@description('Log Analytics Workspace ID')
param logAnalyticsWorkspaceId string

@description('Log Analytics Workspace Key')
@secure()
param logAnalyticsWorkspaceKey string

resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: name
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalyticsWorkspaceId
        sharedKey: logAnalyticsWorkspaceKey
      }
    }
  }
}

output id string = containerAppsEnvironment.id
output name string = containerAppsEnvironment.name
output defaultDomain string = containerAppsEnvironment.properties.defaultDomain
