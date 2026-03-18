@description('CosmosDB account name')
param accountName string

@description('Location for the CosmosDB account')
param location string

@description('Database name')
param databaseName string

resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2024-05-15' = {
  name: accountName
  location: location
  kind: 'GlobalDocumentDB'
  properties: {
    databaseAccountOfferType: 'Standard'
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
    locations: [
      {
        locationName: location
        failoverPriority: 0
        isZoneRedundant: false
      }
    ]
    enableFreeTier: true
    capabilities: [
      {
        name: 'EnableServerless'
      }
    ]
  }
}

resource database 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2024-05-15' = {
  parent: cosmosAccount
  name: databaseName
  properties: {
    resource: {
      id: databaseName
    }
  }
}

resource ordersContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2024-05-15' = {
  parent: database
  name: 'Orders'
  properties: {
    resource: {
      id: 'Orders'
      partitionKey: {
        paths: [
          '/id'
        ]
        kind: 'Hash'
      }
    }
  }
}

resource usersContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2024-05-15' = {
  parent: database
  name: 'Users'
  properties: {
    resource: {
      id: 'Users'
      partitionKey: {
        paths: [
          '/id'
        ]
        kind: 'Hash'
      }
    }
  }
}

resource downloadLinksContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2024-05-15' = {
  parent: database
  name: 'DownloadLinks'
  properties: {
    resource: {
      id: 'DownloadLinks'
      partitionKey: {
        paths: [
          '/id'
        ]
        kind: 'Hash'
      }
    }
  }
}

resource photographerSettingsContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2024-05-15' = {
  parent: database
  name: 'PhotographerSettings'
  properties: {
    resource: {
      id: 'PhotographerSettings'
      partitionKey: {
        paths: [
          '/id'
        ]
        kind: 'Hash'
      }
    }
  }
}

output id string = cosmosAccount.id
output endpoint string = cosmosAccount.properties.documentEndpoint
output connectionString string = 'AccountEndpoint=${cosmosAccount.properties.documentEndpoint};AccountKey=${cosmosAccount.listKeys().primaryMasterKey};'
