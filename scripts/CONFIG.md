# Configuración de Despliegue - Photos Market

Este archivo documenta todas las variables de configuración utilizadas en los scripts de despliegue.

## Recursos de Azure

### Development Environment
```powershell
$ResourceGroup = 'rg-photosmarket-dev'
$ContainerRegistry = 'photosmarketacrdev'
$Environment = 'dev'
$Region = 'eastus'
```

### Container Apps
```powershell
# Backend
$BackendContainerApp = 'photosmarket-backend-dev'
$BackendImageName = 'photosmarket-backend'

# Frontend
$FrontendContainerApp = 'photosmarket-frontend-dev'
$FrontendImageName = 'photosmarket-frontend'
```

## Staging Environment (Ejemplo)

Para configurar un ambiente de staging, modifica los scripts con:

```powershell
$ResourceGroup = 'rg-photosmarket-staging'
$ContainerRegistry = 'photosmarketacrstaging'
$Environment = 'staging'
$Region = 'eastus'

$BackendContainerApp = 'photosmarket-backend-staging'
$FrontendContainerApp = 'photosmarket-frontend-staging'
```

## Production Environment (Ejemplo)

Para producción:

```powershell
$ResourceGroup = 'rg-photosmarket-prod'
$ContainerRegistry = 'photosmarketacrprod'
$Environment = 'prod'
$Region = 'eastus'

$BackendContainerApp = 'photosmarket-backend-prod'
$FrontendContainerApp = 'photosmarket-frontend-prod'
```

## Variables de Entorno Configuradas

### Backend Container App
```bash
FRONTEND_URL=https://{frontend-fqdn}
# Configuradas manualmente en Azure:
# - JWT_SECRET
# - COSMOSDB_CONNECTION_STRING
# - EMAIL_SMTP_PASSWORD
# - GOOGLE_CLIENT_SECRET
```

### Frontend Container App
```bash
VITE_API_URL=https://{backend-fqdn}
```

## Secrets de GitHub Actions

Los workflows de GitHub Actions requieren estos secrets:

```yaml
AZURE_CREDENTIALS: 
  {
    "clientId": "<GUID>",
    "clientSecret": "<GUID>",
    "subscriptionId": "<GUID>",
    "tenantId": "<GUID>"
  }
```

### Crear Service Principal para GitHub Actions

```powershell
# Crear service principal con permisos de contributor
az ad sp create-for-rbac `
  --name "github-actions-photos-market" `
  --role contributor `
  --scopes /subscriptions/{subscription-id}/resourceGroups/rg-photosmarket-dev `
  --sdk-auth

# Guardar el output JSON como secret AZURE_CREDENTIALS en GitHub
```

## Docker Registry

### Container Registry Login
```powershell
# Login local
az acr login --name photosmarketacrdev

# Verificar imágenes disponibles
az acr repository list --name photosmarketacrdev

# Ver tags de una imagen
az acr repository show-tags `
  --name photosmarketacrdev `
  --repository photosmarket-backend `
  --orderby time_desc
```

### Imágenes Docker

```
{registry}.azurecr.io/photosmarket-backend:latest
{registry}.azurecr.io/photosmarket-backend:{tag}

{registry}.azurecr.io/photosmarket-frontend:latest
{registry}.azurecr.io/photosmarket-frontend:{tag}
```

## Permisos Necesarios

### Para ejecutar los scripts localmente:

1. **Azure CLI** con login activo
   ```powershell
   az login
   az account show
   ```

2. **Permisos en Azure**:
   - Contributor en el Resource Group
   - ACRPull/ACRPush en el Container Registry
   - Permissions para leer/actualizar Container Apps

3. **Docker Desktop** instalado y corriendo (para builds)

### Para GitHub Actions:

1. Service Principal con rol Contributor
2. Permisos ACRPush en el Container Registry

## Comandos Útiles de Azure CLI

### Container Apps

```powershell
# Listar Container Apps
az containerapp list --resource-group rg-photosmarket-dev --output table

# Ver detalles de un Container App
az containerapp show `
  --name photosmarket-backend-dev `
  --resource-group rg-photosmarket-dev

# Ver logs en tiempo real
az containerapp logs show `
  --name photosmarket-backend-dev `
  --resource-group rg-photosmarket-dev `
  --follow

# Listar revisiones
az containerapp revision list `
  --name photosmarket-backend-dev `
  --resource-group rg-photosmarket-dev `
  --output table

# Escalar manualmente
az containerapp update `
  --name photosmarket-backend-dev `
  --resource-group rg-photosmarket-dev `
  --min-replicas 2 `
  --max-replicas 10
```

### Container Registry

```powershell
# Listar repositorios
az acr repository list --name photosmarketacrdev

# Eliminar imagen antigua
az acr repository delete `
  --name photosmarketacrdev `
  --image photosmarket-backend:old-tag

# Purgar imágenes antiguas (mantener últimas 3)
az acr repository show-tags `
  --name photosmarketacrdev `
  --repository photosmarket-backend `
  --orderby time_asc `
  --output tsv | Select-Object -Skip 3 | ForEach-Object {
    az acr repository delete `
      --name photosmarketacrdev `
      --image "photosmarket-backend:$_" `
      --yes
}
```

### Resource Group

```powershell
# Ver todos los recursos
az resource list `
  --resource-group rg-photosmarket-dev `
  --output table

# Costos estimados
az consumption usage list `
  --start-date 2026-03-01 `
  --end-date 2026-03-31 `
  --query "[?contains(instanceId, 'rg-photosmarket-dev')]" `
  --output table
```

## Troubleshooting

### El despliegue falla en "az containerapp update"

Verificar que:
1. El Container App existe: `az containerapp show --name {name} --resource-group {rg}`
2. Tienes permisos: `az role assignment list --scope /subscriptions/{sub}/resourceGroups/{rg}`
3. La imagen existe en ACR: `az acr repository show --name {acr} --image {image}:{tag}`

### Docker build falla

1. Verificar Dockerfile en `src/backend/` o `src/frontend/`
2. Verificar que Docker Desktop está corriendo
3. Verificar que tienes espacio en disco
4. Limpiar cache: `docker system prune -a`

### No se puede hacer push a ACR

1. Verificar login: `az acr login --name photosmarketacrdev`
2. Verificar permisos: El usuario debe tener rol `AcrPush`
3. Verificar que ACR permite push: `az acr show --name {acr} --query adminUserEnabled`

## Monitoreo y Logs

### Application Insights
```powershell
# Obtener instrumentation key
az monitor app-insights component show `
  --app photosmarket-appinsights `
  --resource-group rg-photosmarket-dev `
  --query instrumentationKey
```

### Log Analytics
```powershell
# Query con Azure CLI
az monitor log-analytics query `
  --workspace {workspace-id} `
  --analytics-query "ContainerAppConsoleLogs_CL | where ContainerAppName_s == 'photosmarket-backend-dev' | take 20"
```

---

## Referencias

- [Azure Container Apps Documentation](https://docs.microsoft.com/azure/container-apps/)
- [Azure Container Registry Documentation](https://docs.microsoft.com/azure/container-registry/)
- [GitHub Actions for Azure](https://github.com/Azure/actions)
- [Dockerfile Best Practices](https://docs.docker.com/develop/dev-best-practices/)
