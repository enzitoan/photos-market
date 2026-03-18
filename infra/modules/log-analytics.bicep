@description('Log Analytics Workspace name')
param name string

@description('Location for the Log Analytics Workspace')
param location string

@description('SKU for the Log Analytics Workspace')
param sku string = 'PerGB2018'

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: name
  location: location
  properties: {
    sku: {
      name: sku
    }
    retentionInDays: 30
  }
}

output id string = logAnalyticsWorkspace.id
output workspaceId string = logAnalyticsWorkspace.properties.customerId
output workspaceKey string = logAnalyticsWorkspace.listKeys().primarySharedKey
