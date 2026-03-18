# 🚀 Quick Start - GitHub Actions para PhotosMarket

## Paso 1: Crear Service Principal (1 minuto)

Abre Azure Cloud Shell o tu terminal local con Azure CLI y ejecuta:

```bash
az ad sp create-for-rbac \
  --name "photosmarket-github-actions" \
  --role contributor \
  --scopes /subscriptions/$(az account show --query id -o tsv)/resourceGroups/rg-photosmarket-dev \
  --sdk-auth
```

**IMPORTANTE:** Copia TODO el JSON que genera. Lo necesitarás en el siguiente paso.

## Paso 2: Configurar GitHub Secrets (2 minutos)

1. Ve a tu repositorio en GitHub
2. Clic en **Settings** → **Secrets and variables** → **Actions**
3. Clic en **New repository secret**
4. Crea estos 5 secrets:

### Secret 1: AZURE_CREDENTIALS
```
Nombre: AZURE_CREDENTIALS
Valor: [Pega TODO el JSON del service principal del Paso 1]
```

### Secret 2: GOOGLE_OAUTH_CLIENT_ID
```
Nombre: GOOGLE_OAUTH_CLIENT_ID
Valor: [Tu Client ID de Google OAuth - obtenerlo de Google Cloud Console]
```

### Secret 3: GOOGLE_OAUTH_CLIENT_SECRET
```
Nombre: GOOGLE_OAUTH_CLIENT_SECRET
Valor: [Tu Client Secret de Google OAuth - obtenerlo de Google Cloud Console]
```

### Secret 4: JWT_SECRET_KEY
```
Nombre: JWT_SECRET_KEY
Valor: [Genera una clave aleatoria de al menos 32 caracteres]
```

Puedes generar una clave JWT con PowerShell:
```powershell
-join ((48..57) + (65..90) + (97..122) | Get-Random -Count 32 | ForEach-Object {[char]$_})
```

### Secret 5: GOOGLE_DRIVE_ROOT_FOLDER_ID
```
Nombre: GOOGLE_DRIVE_ROOT_FOLDER_ID
Valor: [ID de tu carpeta raíz de Google Drive]
```

Para obtener el ID de la carpeta de Google Drive:
1. Abre la carpeta en Google Drive
2. Mira la URL: `https://drive.google.com/drive/folders/ESTE_ES_EL_ID`
3. Copia el ID que aparece después de `/folders/`

## Paso 3: Dar Permisos al Service Principal (30 segundos)

```bash
# Obtener el Client ID del service principal (del JSON que copiaste)
# Reemplaza con tu Client ID
CLIENT_ID="tu-client-id-aqui"

# Dar permisos para pull de imágenes del Container Registry
ACR_ID=$(az acr show --name photosmarketacrdev --query id -o tsv)
az role assignment create \
  --assignee $CLIENT_ID \
  --role AcrPull \
  --scope $ACR_ID
```

## Paso 4: Commit y Push de los Workflows (1 minuto)

```bash
# Asegúrate de estar en la raíz del proyecto
cd PhotosMarket

# Agregar los archivos de GitHub Actions
git add .github/

# Commit
git commit -m "ci: add GitHub Actions workflows for CI/CD"

# Push a main
git push origin main
```

## Paso 5: Ver el Primer Despliegue (5-10 minutos)

1. Ve a tu repositorio en GitHub
2. Clic en la pestaña **Actions**
3. Verás el workflow **Full Deploy to Azure** ejecutándose
4. Haz clic en él para ver el progreso en tiempo real

## ✅ Verificación Final

Una vez que el workflow termine (aprox. 5-10 minutos):

```bash
# Verificar que los Container Apps estén corriendo
az containerapp list \
  --resource-group rg-photosmarket-dev \
  --query "[].{Name:name, Status:properties.runningStatus}" \
  -o table

# Obtener las URLs
az containerapp show \
  --name photosmarket-backend-dev \
  --resource-group rg-photosmarket-dev \
  --query "properties.configuration.ingress.fqdn" -o tsv

az containerapp show \
  --name photosmarket-frontend-dev \
  --resource-group rg-photosmarket-dev \
  --query "properties.configuration.ingress.fqdn" -o tsv
```

## 🎉 ¡Listo!

A partir de ahora, cada vez que hagas push a `main`:
- ✅ Se construirán automáticamente las imágenes Docker
- ✅ Se subirán a Azure Container Registry
- ✅ Se desplegarán en Azure Container Apps
- ✅ Se ejecutarán health checks

## 🔄 Próximos Pasos

### Desarrollo Local
```bash
# Crear una rama para desarrollo
git checkout -b feature/mi-nueva-funcionalidad

# Hacer cambios
# ... editar código ...

# Commit
git add .
git commit -m "feat: nueva funcionalidad"

# Push
git push origin feature/mi-nueva-funcionalidad

# Crear Pull Request en GitHub
# El CI se ejecutará automáticamente para validar
```

### Despliegue Manual
Si necesitas desplegar manualmente:
1. Ve a **Actions** en GitHub
2. Selecciona "Full Deploy to Azure"
3. Clic en **Run workflow**
4. Selecciona la rama `main`
5. Clic en **Run workflow**

## 🆘 ¿Problemas?

### El workflow falla con "Authentication failed"
→ Verifica que `AZURE_CREDENTIALS` sea el JSON completo del service principal

### Error "unauthorized to access repository"
→ Ejecuta el Paso 3 para dar permisos de AcrPull

### No veo la pestaña Actions en GitHub
→ Ve a Settings → Actions → General y habilita "Allow all actions"

### El Container App no se actualiza
→ Verifica los logs en Azure:
```bash
az containerapp logs show \
  --name photosmarket-backend-dev \
  --resource-group rg-photosmarket-dev \
  --follow
```

## 📞 Comandos Útiles

```bash
# Ver estado de workflows
gh run list

# Ver logs de la última ejecución
gh run view --log

# Cancelar workflow en ejecución
gh run cancel <run-id>

# Re-ejecutar workflow fallido
gh run rerun <run-id>
```

¿Todo funcionando? 🎊 Ahora tienes CI/CD completamente automatizado!
