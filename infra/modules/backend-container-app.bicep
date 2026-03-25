@description('Container App name')
param name string

@description('Location for the Container App')
param location string

@description('Container Apps Environment ID')
param containerAppsEnvironmentId string

@description('Container image')
param containerImage string

@description('Container Registry name')
param containerRegistryName string

@description('Key Vault URI')
param keyVaultUri string

@description('Google Drive Root Folder ID')
param googleDriveRootFolderId string

@description('Frontend URL for CORS')
param frontendUrl string = ''

@description('Enable email sending')
param emailEnabled bool = true

resource containerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: name
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    managedEnvironmentId: containerAppsEnvironmentId
    configuration: {
      ingress: {
        external: true
        targetPort: 8080
        transport: 'auto'
        allowInsecure: false
      }
      registries: [
        {
          server: '${containerRegistryName}.azurecr.io'
          identity: 'system'
        }
      ]
      secrets: [
        {
          name: 'google-oauth-client-id'
          keyVaultUrl: '${keyVaultUri}secrets/GoogleOAuthClientId'
          identity: 'system'
        }
        {
          name: 'google-oauth-client-secret'
          keyVaultUrl: '${keyVaultUri}secrets/GoogleOAuthClientSecret'
          identity: 'system'
        }
        {
          name: 'jwt-secret-key'
          keyVaultUrl: '${keyVaultUri}secrets/JwtSecretKey'
          identity: 'system'
        }
        {
          name: 'cosmosdb-connection-string'
          keyVaultUrl: '${keyVaultUri}secrets/CosmosDbConnectionString'
          identity: 'system'
        }
        {
          name: 'google-drive-credentials'
          keyVaultUrl: '${keyVaultUri}secrets/GoogleDriveCredentials'
          identity: 'system'
        }
        {
          name: 'email-api-key'
          keyVaultUrl: '${keyVaultUri}secrets/EmailApiKey'
          identity: 'system'
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'backend'
          image: containerImage
          resources: {
            cpu: json('0.5')
            memory: '1.0Gi'
          }
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: 'Production'
            }
            {
              name: 'FRONTEND_URL'
              value: frontendUrl
            }
            {
              name: 'ASPNETCORE_URLS'
              value: 'http://+:8080'
            }
            {
              name: 'UseRealCosmosDb'
              value: 'true'
            }
            {
              name: 'ConnectionStrings__CosmosDb'
              secretRef: 'cosmosdb-connection-string'
            }
            {
              name: 'CosmosDb__DatabaseName'
              value: 'PhotosMarketDb'
            }
            {
              name: 'CosmosDb__ContainerNames__Orders'
              value: 'Orders'
            }
            {
              name: 'CosmosDb__ContainerNames__Users'
              value: 'Users'
            }
            {
              name: 'CosmosDb__ContainerNames__DownloadLinks'
              value: 'DownloadLinks'
            }
            {
              name: 'CosmosDb__ContainerNames__PhotographerSettings'
              value: 'PhotographerSettings'
            }
            {
              name: 'GoogleDrive__RootFolderId'
              value: googleDriveRootFolderId
            }
            {
              name: 'GoogleDrive__ApplicationName'
              value: 'PhotosMarket'
            }
            {
              name: 'GoogleDrive__CredentialsJson'
              secretRef: 'google-drive-credentials'
            }
            {
              name: 'GoogleOAuth__ClientId'
              secretRef: 'google-oauth-client-id'
            }
            {
              name: 'GoogleOAuth__ClientSecret'
              secretRef: 'google-oauth-client-secret'
            }
            {
              name: 'GoogleOAuth__Scopes__0'
              value: 'https://www.googleapis.com/auth/userinfo.email'
            }
            {
              name: 'GoogleOAuth__Scopes__1'
              value: 'https://www.googleapis.com/auth/userinfo.profile'
            }
            {
              name: 'GoogleOAuth__Scopes__2'
              value: 'openid'
            }
            {
              name: 'Jwt__SecretKey'
              secretRef: 'jwt-secret-key'
            }
            {
              name: 'Jwt__Issuer'
              value: 'PhotosMarketAPI'
            }
            {
              name: 'Jwt__Audience'
              value: 'PhotosMarketClient'
            }
            {
              name: 'Jwt__ExpirationInMinutes'
              value: '60'
            }
            {
              name: 'Email__Enabled'
              value: string(emailEnabled)
            }
            {
              name: 'Email__ApiKey'
              secretRef: 'email-api-key'
            }
            {
              name: 'Email__SenderEmail'
              value: 'onboarding@resend.dev'
            }
            {
              name: 'Email__SenderName'
              value: 'Photos Market'
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 10
        rules: [
          {
            name: 'http-scaling'
            http: {
              metadata: {
                concurrentRequests: '100'
              }
            }
          }
        ]
      }
    }
  }
}

output id string = containerApp.id
output name string = containerApp.name
output fqdn string = containerApp.properties.configuration.ingress.fqdn
output principalId string = containerApp.identity.principalId
