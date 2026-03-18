# Configuración de GitHub Actions para PhotosMarket

Este documento explica cómo configurar GitHub Actions para el despliegue continuo (CI/CD) de PhotosMarket en Azure.

## 📋 Workflows Creados

### 1. **CI - Build and Test** (`ci.yml`)
- **Trigger**: Pull requests y pushes a ramas que no sean `main`
- **Acciones**:
  - Compila el backend (.NET)
  - Compila el frontend (Vue.js)
  - Prueba la construcción de imágenes Docker
  - Ejecuta tests unitarios (si existen)

### 2. **Deploy Backend** (`deploy-backend.yml`)
- **Trigger**: Push a `main` cuando hay cambios en `src/backend/` o manual
- **Acciones**:
  - Construye la imagen Docker del backend
  - La sube a Azure Container Registry
  - Despliega en Azure Container Apps

### 3. **Deploy Frontend** (`deploy-frontend.yml`)
- **Trigger**: Push a `main` cuando hay cambios en `src/frontend/` o manual
- **Acciones**:
  - Construye la imagen Docker del frontend
  - La sube a Azure Container Registry
  - Despliega en Azure Container Apps

### 4. **Deploy Infrastructure** (`deploy-infra.yml`)
- **Trigger**: Push a `main` cuando hay cambios en `infra/` o manual
- **Acciones**:
  - Despliega la infraestructura Bicep en Azure
  - Crea o actualiza todos los recursos de Azure

### 5. **Full Deploy** (`full-deploy.yml`)
- **Trigger**: Push a `main` o manual
- **Acciones**:
  - Despliega backend y frontend juntos
  - Ejecuta health checks
  - Proporciona un resumen del despliegue

## 🔐 Secrets Requeridos en GitHub

Debes configurar los siguientes secrets en tu repositorio de GitHub:

### 1. Crear Service Principal de Azure

Ejecuta este comando en tu terminal (reemplaza `SUBSCRIPTION_ID` con tu ID de suscripción):

```bash
# Obtener tu subscription ID
az account show --query id -o tsv

# Crear service principal
az ad sp create-for-rbac \
  --name "photosmarket-github-actions" \
  --role contributor \
  --scopes /subscriptions/YOUR_SUBSCRIPTION_ID/resourceGroups/rg-photosmarket-dev \
  --sdk-auth
```

Este comando generará un JSON como este:

```json
{
  "clientId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "clientSecret": "xxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
  "subscriptionId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "tenantId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "activeDirectoryEndpointUrl": "https://login.microsoftonline.com",
  "resourceManagerEndpointUrl": "https://management.azure.com/",
  "activeDirectoryGraphResourceId": "https://graph.windows.net/",
  "sqlManagementEndpointUrl": "https://management.core.windows.net:8443/",
  "galleryEndpointUrl": "https://gallery.azure.com/",
  "managementEndpointUrl": "https://management.core.windows.net/"
}
```

### 2. Configurar Secrets en GitHub

Ve a tu repositorio en GitHub:
1. **Settings** → **Secrets and variables** → **Actions** → **New repository secret**
2. Crea los siguientes secrets:

| Secret Name | Valor | Descripción |
|------------|-------|-------------|
| `AZURE_CREDENTIALS` | Todo el JSON del service principal | Credenciales de Azure para autenticación |
| `GOOGLE_OAUTH_CLIENT_ID` | Tu Client ID de Google | ID de cliente OAuth de Google |
| `GOOGLE_OAUTH_CLIENT_SECRET` | Tu Client Secret de Google | Secret de cliente OAuth de Google |
| `JWT_SECRET_KEY` | Tu clave secreta JWT | Clave para firmar tokens JWT (mínimo 32 caracteres) |
| `GOOGLE_DRIVE_ROOT_FOLDER_ID` | ID de tu carpeta de Google Drive | ID de la carpeta raíz de Google Drive |

### 3. Ejemplo Visual

```bash
# En tu terminal de Azure
az ad sp create-for-rbac \
  --name "photosmarket-github-actions" \
  --role contributor \
  --scopes /subscriptions/$(az account show --query id -o tsv)/resourceGroups/rg-photosmarket-dev \
  --sdk-auth
```

Copia **TODO** el JSON que genera y pégalo como el valor de `AZURE_CREDENTIALS` en GitHub.

## 🚀 Cómo Usar los Workflows

### Despliegue Automático
1. Haz cambios en tu código
2. Commit y push a la rama `main`:
   ```bash
   git add .
   git commit -m "feat: nueva funcionalidad"
   git push origin main
   ```
3. GitHub Actions automáticamente:
   - Detectará qué cambió (backend, frontend o infra)
   - Ejecutará solo los workflows necesarios
   - Desplegará a Azure

### Despliegue Manual
1. Ve a **Actions** en tu repositorio de GitHub
2. Selecciona el workflow que deseas ejecutar
3. Haz clic en **Run workflow**
4. Selecciona la rama y confirma

## 📊 Monitorear Despliegues

### Ver el Estado en GitHub
1. Ve a la pestaña **Actions** en tu repositorio
2. Verás una lista de todos los workflows ejecutados
3. Haz clic en cualquier ejecución para ver detalles

### Ver Estado en Azure
```bash
# Estado de los Container Apps
az containerapp list \
  --resource-group rg-photosmarket-dev \
  --query "[].{Name:name, Status:properties.runningStatus, Revision:properties.latestRevisionName}" \
  -o table

# Logs del Backend
az containerapp logs show \
  --name photosmarket-backend-dev \
  --resource-group rg-photosmarket-dev \
  --follow

# Logs del Frontend
az containerapp logs show \
  --name photosmarket-frontend-dev \
  --resource-group rg-photosmarket-dev \
  --follow
```

## 🔄 Flujo de Trabajo Recomendado

### Para Desarrollo
```bash
# 1. Crear rama de feature
git checkout -b feature/nueva-funcionalidad

# 2. Hacer cambios y commit
git add .
git commit -m "feat: descripción de cambios"

# 3. Push a la rama
git push origin feature/nueva-funcionalidad

# 4. Crear Pull Request en GitHub
# El workflow CI se ejecutará automáticamente

# 5. Una vez aprobado, merge a main
# Los workflows de deploy se ejecutarán automáticamente
```

### Para Hotfixes
```bash
# 1. Crear rama de hotfix
git checkout -b hotfix/correccion-urgente

# 2. Hacer cambios y commit
git add .
git commit -m "fix: corrección urgente"

# 3. Push y crear PR
git push origin hotfix/correccion-urgente

# 4. Merge a main para despliegue automático
```

## 🛠️ Personalización

### Cambiar Nombres de Recursos
Si necesitas cambiar los nombres de recursos en Azure, edita las variables `env` en cada workflow:

```yaml
env:
  AZURE_RESOURCE_GROUP: rg-photosmarket-dev
  AZURE_CONTAINER_REGISTRY: photosmarketacrdev
  BACKEND_CONTAINER_APP: photosmarket-backend-dev
  FRONTEND_CONTAINER_APP: photosmarket-frontend-dev
  AZURE_REGION: eastus
```

### Agregar Ambientes (Staging, Production)
1. Duplica los workflows
2. Cambia los nombres de recursos
3. Crea secrets específicos para cada ambiente
4. Usa diferentes ramas para diferentes ambientes:
   - `main` → Production
   - `staging` → Staging
   - `develop` → Development

### Notificaciones
Puedes agregar notificaciones de Slack, Teams, o email añadiendo steps adicionales:

```yaml
- name: Notify on Success
  if: success()
  uses: actions/slack-notification@v1
  with:
    webhook: ${{ secrets.SLACK_WEBHOOK }}
    message: "✅ Deployment successful!"
```

## 🐛 Troubleshooting

### Error: "az: command not found"
Los runners de GitHub Actions ya tienen Azure CLI instalado. Si ves este error, verifica que estés usando `runs-on: ubuntu-latest`.

### Error: "unauthorized to access repository"
Verifica que `AZURE_CREDENTIALS` esté configurado correctamente con el JSON completo del service principal.

### Error: "Image pull authentication failed"
El service principal necesita permisos `AcrPull` en el Container Registry:

```bash
# Obtener el ID del ACR
ACR_ID=$(az acr show --name photosmarketacrdev --query id -o tsv)

# Asignar rol AcrPull al service principal
az role assignment create \
  --assignee YOUR_SERVICE_PRINCIPAL_CLIENT_ID \
  --role AcrPull \
  --scope $ACR_ID
```

### Error de Secrets en Container Apps
Los secrets del Key Vault ya están configurados. Si encuentras errores, verifica:

```bash
az containerapp show \
  --name photosmarket-backend-dev \
  --resource-group rg-photosmarket-dev \
  --query "properties.configuration.secrets"
```

## 📚 Recursos Adicionales

- [GitHub Actions Docs](https://docs.github.com/en/actions)
- [Azure Container Apps CI/CD](https://learn.microsoft.com/en-us/azure/container-apps/github-actions)
- [Azure CLI Reference](https://learn.microsoft.com/en-us/cli/azure/)

## ✅ Checklist de Configuración

- [ ] Service Principal creado
- [ ] `AZURE_CREDENTIALS` configurado en GitHub Secrets
- [ ] `GOOGLE_OAUTH_CLIENT_ID` configurado
- [ ] `GOOGLE_OAUTH_CLIENT_SECRET` configurado
- [ ] `JWT_SECRET_KEY` configurado
- [ ] `GOOGLE_DRIVE_ROOT_FOLDER_ID` configurado
- [ ] Permisos de AcrPull asignados al service principal
- [ ] Primer workflow ejecutado exitosamente
- [ ] URLs de producción verificadas
