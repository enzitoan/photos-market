# 🏗️ Deploy Infrastructure Workflow

## Descripción

Workflow de GitHub Actions para desplegar y mantener la infraestructura de Azure para PhotosMarket.

## 📋 Qué Despliega

Este workflow crea/actualiza los siguientes recursos en Azure:

### Recursos Principales
- **Container Apps Environment** - Entorno para las aplicaciones containerizadas
- **Container Registry (ACR)** - Registro de imágenes Docker
- **CosmosDB** - Base de datos NoSQL
- **Key Vault** - Almacenamiento seguro de secrets
- **Log Analytics** - Workspace para logs y métricas

### Container Apps
- **Frontend Container App** - Aplicación Vue.js
- **Backend Container App** - API .NET

### Networking
- Ingress configurado para ambas apps
- HTTPS automático
- CORS configurado

## 🎯 Cuándo Usar

### Escenario 1: Primera Vez (Setup Inicial)
```bash
# Ejecutar manualmente desde GitHub Actions
# Actions → Deploy Infrastructure → Run workflow
# - environment: dev
# - validation_only: false
```

### Escenario 2: Cambios en Infraestructura
```bash
# Editar archivos Bicep
vim infra/modules/backend-container-app.bicep

# Push dispara automáticamente el workflow
git add infra/
git commit -m "feat: Update backend resources"
git push origin main
```

### Escenario 3: Validar Antes de Desplegar
```bash
# Ejecutar manualmente con validation_only=true
# Verifica que el Bicep sea válido sin hacer cambios
```

### Escenario 4: Actualizar Secrets
```bash
# Actualizar secrets en GitHub
# Ejecutar workflow manualmente para propagarlos a Azure
```

## ⚙️ Inputs del Workflow (Manual Dispatch)

| Input | Tipo | Default | Descripción |
|-------|------|---------|-------------|
| `environment` | choice | `dev` | Environment a desplegar: dev, staging, prod |
| `validation_only` | boolean | `false` | Solo validar, no desplegar |
| `force_update_oauth` | boolean | `true` | Actualizar configuración OAuth después del deployment |

## 📝 Secrets Requeridos

Debes configurar estos secrets en GitHub (Settings → Secrets):

| Secret | Descripción | Ejemplo |
|--------|-------------|---------|
| `AZURE_CREDENTIALS` | Service Principal JSON | Ver sección abajo |
| `GOOGLE_OAUTH_CLIENT_ID` | OAuth Client ID de Google | `12345-xxx.apps.googleusercontent.com` |
| `GOOGLE_OAUTH_CLIENT_SECRET` | OAuth Client Secret de Google | `GOCSPX-xxx` |
| `JWT_SECRET_KEY` | Clave secreta para JWT | Mínimo 64 caracteres |
| `GOOGLE_DRIVE_ROOT_FOLDER_ID` | ID de carpeta raíz en Google Drive | `1JoezTDvr...` |
| `GOOGLE_DRIVE_CREDENTIALS` | JSON de Service Account | `{"type":"service_account",...}` |

### Cómo Crear AZURE_CREDENTIALS

```bash
# Crear Service Principal con permisos de Contributor
az ad sp create-for-rbac \
  --name "sp-photosmarket-github-actions" \
  --role contributor \
  --scopes /subscriptions/{SUBSCRIPTION-ID}/resourceGroups/rg-photosmarket-dev \
  --sdk-auth

# Copiar todo el JSON output y pegarlo en GitHub Secret
```

## 🔄 Flujo de Ejecución

```
1. validate-secrets Job
   ├─ Verificar que todos los secrets existan
   └─ Fallar si falta alguno

2. deploy-infrastructure Job
   ├─ Login a Azure
   ├─ Crear/verificar Resource Group
   ├─ Validar template Bicep
   ├─ [Si validation_only=false]
   │  ├─ Desplegar infraestructura
   │  ├─ Obtener outputs (URLs, FQDNs)
   │  ├─ Mostrar resumen
   │  └─ Verificar configuración OAuth
   └─ [Si validation_only=true]
      └─ Mostrar resumen de validación

3. update-service-urls Job
   ├─ Actualizar Frontend con Backend URL
   ├─ Actualizar Backend con Frontend URL
   └─ Mostrar configuración final
```

## 📊 Outputs

Al finalizar exitosamente, el workflow muestra:

```
╔═══════════════════════════════════════════════════════════╗
║       INFRASTRUCTURE DEPLOYMENT COMPLETED                 ║
╚═══════════════════════════════════════════════════════════╝

🌐 Application URLs:
   Frontend: https://photosmarket-frontend-dev.jollywater-xxx.eastus.azurecontainerapps.io
   Backend:  https://photosmarket-backend-dev.jollywater-xxx.eastus.azurecontainerapps.io

📦 Resources:
   [Lista de Container Apps con status]

🔐 OAuth Configuration:
   Frontend FQDN: photosmarket-frontend-dev.jollywater-xxx.eastus.azurecontainerapps.io
   Redirect URI:  https://photosmarket-frontend-dev.jollywater-xxx.eastus.azurecontainerapps.io/callback

⚠️  IMPORTANT: Configure this redirect URI in Google Cloud Console
```

## ✅ Verificación Post-Deployment

### 1. Verificar que los recursos existan

```bash
# Listar Container Apps
az containerapp list \
  --resource-group rg-photosmarket-dev \
  -o table

# Verificar CosmosDB
az cosmosdb list \
  --resource-group rg-photosmarket-dev \
  -o table

# Verificar Key Vault
az keyvault list \
  --resource-group rg-photosmarket-dev \
  -o table
```

### 2. Verificar secrets en Key Vault

```bash
# Listar secrets
az keyvault secret list \
  --vault-name photosmarket-kv-dev \
  -o table

# Debe incluir:
# - GoogleOAuthClientId
# - GoogleOAuthClientSecret
# - JwtSecretKey
# - CosmosDbConnectionString
# - GoogleDriveCredentials
```

### 3. Verificar configuración OAuth

```bash
# Ejecutar script de verificación
cd scripts
.\Fix-AzureOAuthConfig.ps1 -ResourceGroupName rg-photosmarket-dev

# Debe mostrar:
# ✅ FRONTEND_URL configurada
# ✅ 3 scopes configurados
```

### 4. Configurar Google Cloud Console

1. Ve a: https://console.cloud.google.com/
2. APIs & Services → Credentials
3. Tu OAuth 2.0 Client ID
4. En "Authorized redirect URIs", agrega:
   ```
   https://[frontend-fqdn-mostrado-en-output]/callback
   ```

## 🐛 Troubleshooting

### Error: "Missing required secrets"

**Causa:** Uno o más secrets no están configurados en GitHub.

**Solución:**
1. Ve a GitHub → Settings → Secrets and variables → Actions
2. Verifica que todos los secrets de la tabla anterior estén configurados
3. Vuelve a ejecutar el workflow

### Error: "Bicep validation failed"

**Causa:** Syntax error en los archivos Bicep.

**Solución:**
1. Ejecuta localmente la validación:
   ```bash
   az bicep build --file ./infra/main.bicep
   ```
2. Corrige los errores reportados
3. Commit y push

### Error: "Deployment failed - Quota exceeded"

**Causa:** No hay cuota suficiente en la suscripción.

**Solución:**
1. Verifica las quotas:
   ```bash
   az vm list-usage --location eastus -o table
   ```
2. Solicita aumento de quota o cambia de región

### Warning: "FRONTEND_URL mismatch"

**Causa:** El FRONTEND_URL en el backend no coincide con el FQDN real.

**Solución:**
- El workflow lo corrige automáticamente si `force_update_oauth=true`
- Si no, ejecuta manualmente:
  ```bash
  cd scripts
  .\Fix-AzureOAuthConfig.ps1 -ResourceGroupName rg-photosmarket-dev
  ```

### OAuth error después del deployment

**Causa:** Google Cloud Console no tiene el redirect URI correcto.

**Solución:**
1. Copia el redirect URI del output del workflow
2. Agrégalo en Google Cloud Console
3. Espera 1-2 minutos para que se propague
4. Prueba el login nuevamente

## 💡 Best Practices

### 1. Siempre Valida Primero
```bash
# Para cambios mayores, primero valida
Run workflow → validation_only = true

# Si pasa, entonces despliega
Run workflow → validation_only = false
```

### 2. Usa Environments Separados
```bash
# Dev para desarrollo
environment: dev

# Staging para testing
environment: staging

# Prod para producción
environment: prod
```

### 3. Monitorea el Deployment
- Watch los logs en tiempo real en GitHub Actions
- Verifica los outputs al finalizar
- Confirma que las apps funcionen después del deployment

### 4. Documenta Cambios Importantes
```bash
git commit -m "feat: Add Redis cache to infrastructure

- Added Redis module
- Updated backend to use cache
- Updated Key Vault with Redis connection string"
```

### 5. Backup Antes de Cambios Mayores
```bash
# Backup CosmosDB
az cosmosdb sql database export \
  --account-name photosmarket-cosmos-dev \
  --resource-group rg-photosmarket-dev

# Backup Key Vault secrets
az keyvault secret backup \
  --vault-name photosmarket-kv-dev
```

## 📚 Archivos Relacionados

| Archivo | Propósito |
|---------|-----------|
| `infra/main.bicep` | Template principal de infraestructura |
| `infra/main.bicepparam` | Parámetros para el deployment |
| `infra/modules/*.bicep` | Módulos individuales de recursos |
| `scripts/Fix-AzureOAuthConfig.ps1` | Script de corrección OAuth |
| `scripts/Update-ServiceUrls.ps1` | Script de actualización de URLs |

## 🔗 Ver También

- [Deployment Flow](../../docs/DEPLOYMENT-FLOW.md) - Flujo completo de deployment
- [OAuth Troubleshooting](../../docs/TROUBLESHOOTING-OAUTH.md) - Solución de errores OAuth
- [Deploy Applications Workflow](deploy-to-azure.yml) - Workflow de aplicaciones

## 🎯 Casos de Uso Comunes

### Agregar un Nuevo Secret
```bash
# 1. Agregar secret en GitHub
# 2. Actualizar main.bicep para aceptar el parámetro
# 3. Actualizar main.bicepparam para leerlo
# 4. Actualizar Key Vault module para almacenarlo
# 5. Ejecutar workflow
```

### Cambiar SKU de un Recurso
```bash
# 1. Editar el módulo correspondiente
vim infra/modules/backend-container-app.bicep

# 2. Cambiar recursos (CPU, memoria)
cpu: json('1.0')  # antes: 0.5
memory: '2.0Gi'   # antes: 1.0Gi

# 3. Commit y push (deploy automático)
git add infra/
git commit -m "feat: Increase backend resources"
git push origin main
```

### Agregar un Nuevo Módulo
```bash
# 1. Crear nuevo módulo
touch infra/modules/redis-cache.bicep

# 2. Agregar en main.bicep
module redis 'modules/redis-cache.bicep' = { ... }

# 3. Actualizar dependencias
module backendApp 'modules/backend-container-app.bicep' = {
  dependsOn: [redis]
  ...
}

# 4. Push y deploy
```

---

✅ **Este workflow asegura que tu infraestructura esté siempre sincronizada y correctamente configurada.**
