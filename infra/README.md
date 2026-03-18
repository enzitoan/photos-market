# PhotosMarket - Infraestructura Azure (Bicep)

Este directorio contiene los templates de Bicep para desplegar PhotosMarket en Azure.

## 📁 Estructura de Archivos

```
infra/
├── main.bicep                          # Template principal
├── main.bicepparam                     # Parámetros de despliegue
└── modules/
    ├── container-registry.bicep        # Azure Container Registry
    ├── log-analytics.bicep             # Log Analytics Workspace
    ├── cosmos-db.bicep                 # Cosmos DB con contenedores
    ├── key-vault.bicep                 # Key Vault para secretos
    ├── container-apps-environment.bicep # Container Apps Environment
    ├── backend-container-app.bicep     # Backend Container App
    └── frontend-container-app.bicep    # Frontend Container App
```

## 🏗️ Arquitectura de Infraestructura

```
┌─────────────────────────────────────────────────────────┐
│                   Azure Resource Group                  │
│                                                          │
│  ┌────────────────────────────────────────────────┐    │
│  │     Azure Container Apps Environment           │    │
│  │  ┌──────────────────┐  ┌──────────────────┐   │    │
│  │  │  Backend App     │  │  Frontend App    │   │    │
│  │  │  (ASP.NET Core)  │  │  (Vue.js+Nginx)  │   │    │
│  │  │  - Port 8080     │  │  - Port 80       │   │    │
│  │  │  - Auto-scale    │  │  - Auto-scale    │   │    │
│  │  └──────────────────┘  └──────────────────┘   │    │
│  └────────────────────────────────────────────────┘    │
│                                                          │
│  ┌────────────────┐  ┌──────────────┐  ┌──────────┐   │
│  │ Container      │  │ Azure        │  │ Azure    │   │
│  │ Registry (ACR) │  │ Cosmos DB    │  │ Key Vault│   │
│  └────────────────┘  └──────────────┘  └──────────┘   │
│                                                          │
│  ┌────────────────────────────────────────────────┐    │
│  │       Log Analytics Workspace                  │    │
│  └────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────┘
```

## 🚀 Uso

### Pre-requisitos

- Azure CLI instalado
- Cuenta de Azure activa
- Permisos de Contributor en la suscripción
- Docker instalado (para construir imágenes)

### Despliegue Rápido

```powershell
# Configurar variables de entorno requeridas
$env:GOOGLE_OAUTH_CLIENT_ID = "tu-client-id"
$env:GOOGLE_OAUTH_CLIENT_SECRET = "tu-client-secret"
$env:JWT_SECRET_KEY = "tu-jwt-secret-minimo-32-caracteres"

# Ejecutar script de despliegue
cd ../scripts
.\deploy-azure.ps1 -ResourceGroupName "rg-photosmarket-dev" -Environment "dev"
```

### Despliegue Manual

```bash
# Login a Azure
az login

# Crear Resource Group
az group create \
  --name rg-photosmarket-dev \
  --location eastus

# Desplegar infraestructura
az deployment group create \
  --resource-group rg-photosmarket-dev \
  --template-file main.bicep \
  --parameters main.bicepparam \
  --parameters environmentName=dev
```

## ⚙️ Parámetros de Configuración

### main.bicepparam

Edita este archivo para configurar tu despliegue:

```bicep
param environmentName = 'dev'              // dev, staging, prod
param appName = 'photosmarket'             // Nombre base de la app
param location = 'eastus'                  // Región de Azure
param googleDriveRootFolderId = 'YOUR_ID'  // ID carpeta Google Drive

// Secretos - Se obtienen de variables de entorno
param googleOAuthClientId = readEnvironmentVariable('GOOGLE_OAUTH_CLIENT_ID', '')
param googleOAuthClientSecret = readEnvironmentVariable('GOOGLE_OAUTH_CLIENT_SECRET', '')
param jwtSecretKey = readEnvironmentVariable('JWT_SECRET_KEY', '')
```

## 📦 Recursos Desplegados

### 1. Container Registry (ACR)

**Nombre**: `photosmarketacr{environment}`  
**SKU**: Basic  
**Propósito**: Almacenar imágenes Docker del backend y frontend

```bicep
// Características:
- Admin user habilitado
- Public network access
- Autenticación mediante identidad administrada
```

### 2. Log Analytics Workspace

**Nombre**: `photosmarket-logs-{environment}`  
**Retención**: 30 días  
**Propósito**: Centralizar logs de todas las Container Apps

### 3. Cosmos DB

**Nombre**: `photosmarket-cosmos-{environment}`  
**Tipo**: Serverless  
**Propósito**: Base de datos NoSQL para la aplicación

**Contenedores creados:**
- `Orders` - Pedidos de clientes
- `Users` - Usuarios registrados
- `DownloadLinks` - Enlaces de descarga
- `PhotographerSettings` - Configuración del fotógrafo

**Características:**
- Nivel gratuito habilitado (si disponible)
- Consistency level: Session
- Partition key: `/id` para todos los contenedores

### 4. Key Vault

**Nombre**: `photosmarket-kv-{environment}`  
**SKU**: Standard  
**Propósito**: Almacenar secretos de forma segura

**Secretos almacenados:**
- `GoogleOAuthClientId`
- `GoogleOAuthClientSecret`
- `JwtSecretKey`
- `CosmosDbConnectionString`

**Seguridad:**
- RBAC habilitado
- Acceso mediante Managed Identity
- No acceso por clave

### 5. Container Apps Environment

**Nombre**: `photosmarket-env-{environment}`  
**Propósito**: Entorno compartido para Container Apps

**Características:**
- Integrado con Log Analytics
- Dominio por defecto: `*.{region}.azurecontainerapps.io`

### 6. Backend Container App

**Nombre**: `photosmarket-backend`  
**Imagen**: `{acr}.azurecr.io/photosmarket-backend:latest`  
**Recursos**: 0.5 CPU, 1.0 Gi Memory  

**Escalado:**
- Mínimo: 1 réplica
- Máximo: 10 réplicas
- Regla: 100 peticiones concurrentes

**Variables de entorno:**
```
ASPNETCORE_ENVIRONMENT=Production
UseRealCosmosDb=true
ConnectionStrings__CosmosDb=[Key Vault]
GoogleOAuth__ClientId=[Key Vault]
GoogleOAuth__ClientSecret=[Key Vault]
Jwt__SecretKey=[Key Vault]
GoogleDrive__RootFolderId={parameter}
```

**Seguridad:**
- Managed Identity habilitada
- Acceso a Key Vault mediante RBAC
- HTTPS habilitado

### 7. Frontend Container App

**Nombre**: `photosmarket-frontend`  
**Imagen**: `{acr}.azurecr.io/photosmarket-frontend:latest`  
**Recursos**: 0.25 CPU, 0.5 Gi Memory  

**Escalado:**
- Mínimo: 1 réplica
- Máximo: 5 réplicas
- Regla: 50 peticiones concurrentes

**Variables de entorno:**
```
VITE_API_URL=https://{backend-fqdn}
```

## 🔐 Seguridad

### Managed Identities

Ambas Container Apps usan System-Assigned Managed Identity para:
- Acceder a Azure Container Registry
- Leer secretos desde Key Vault
- Evitar almacenar credenciales en código

### Key Vault Access

El template automáticamente configura:
1. Container Apps con Managed Identity
2. Role Assignment para acceder a Key Vault
3. Referencias a secretos desde variables de entorno

### HTTPS/TLS

- Todas las Container Apps usan HTTPS por defecto
- Certificados gestionados automáticamente por Azure
- HTTP deshabilitado en producción

## 📊 Monitoreo

### Application Insights

Los logs se envían automáticamente a Log Analytics:

```bash
# Ver logs del backend
az containerapp logs show \
  --name photosmarket-backend \
  --resource-group rg-photosmarket-dev \
  --follow

# Ver logs del frontend
az containerapp logs show \
  --name photosmarket-frontend \
  --resource-group rg-photosmarket-dev \
  --follow
```

### Queries KQL Útiles

```kusto
// Errores del backend (últimas 24h)
ContainerAppConsoleLogs_CL
| where ContainerName_s == "backend"
| where Log_s contains "error"
| where TimeGenerated > ago(24h)
| project TimeGenerated, Log_s

// Requests HTTP del frontend
ContainerAppConsoleLogs_CL
| where ContainerName_s == "frontend"
| where TimeGenerated > ago(1h)
| summarize count() by bin(TimeGenerated, 5m)
```

## 💰 Costos Estimados

**Nivel gratuito de Azure:**
- Cosmos DB: Primeros 1000 RU/s gratis
- Container Apps: Primeros 180,000 vCPU-segundos/mes gratis

**Estimación para ambiente dev (uso bajo):**
- Container Apps: $5-10/mes
- Cosmos DB: $0-5/mes (con nivel gratuito)
- Container Registry: $5/mes (Basic)
- Key Vault: $0-1/mes
- Log Analytics: $0-5/mes

**Total estimado: ~$10-25/mes** (puede ser menor con nivel gratuito)

## 🔄 Actualización de la Aplicación

### Actualizar solo el código

```powershell
# Reconstruir y subir imágenes
cd scripts
.\build-docker.ps1 -Push -Registry photosmarketacrdev

# Reiniciar Container Apps
az containerapp update \
  --name photosmarket-backend \
  --resource-group rg-photosmarket-dev

az containerapp update \
  --name photosmarket-frontend \
  --resource-group rg-photosmarket-dev
```

### Actualizar infraestructura

```powershell
cd scripts
.\deploy-azure.ps1 -ResourceGroupName "rg-photosmarket-dev" -Environment "dev"
```

## 🗑️ Limpieza de Recursos

Para eliminar toda la infraestructura:

```bash
az group delete --name rg-photosmarket-dev --yes --no-wait
```

## 🌍 Multi-Región

Para desplegar en múltiples regiones, crea varios Resource Groups:

```powershell
# Región 1: East US
.\deploy-azure.ps1 -ResourceGroupName "rg-photosmarket-eastus" -Location "eastus" -Environment "prod"

# Región 2: West Europe
.\deploy-azure.ps1 -ResourceGroupName "rg-photosmarket-westeu" -Location "westeurope" -Environment "prod"
```

Luego configura Azure Front Door o Traffic Manager para distribución global.

## 📚 Referencias

- [Azure Container Apps Documentation](https://learn.microsoft.com/azure/container-apps/)
- [Azure Bicep Documentation](https://learn.microsoft.com/azure/azure-resource-manager/bicep/)
- [Azure Cosmos DB Serverless](https://learn.microsoft.com/azure/cosmos-db/serverless)
- [Azure Key Vault Best Practices](https://learn.microsoft.com/azure/key-vault/general/best-practices)
- [Container Apps Managed Identity](https://learn.microsoft.com/azure/container-apps/managed-identity)
