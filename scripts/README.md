# Scripts de Despliegue - Photos Market

Este directorio contiene scripts para automatizar el despliegue y gestión de Photos Market en Azure.

## 📜 Scripts Disponibles

### 1. Deploy-PhotosMarket.ps1
**Script principal de despliegue** - Replica la funcionalidad de los GitHub Actions para desplegar la aplicación a Azure Container Apps.

### 2. Update-ServiceUrls.ps1
**Actualización rápida de URLs** - Actualiza las variables de entorno entre servicios sin hacer rebuild.

### 3. Get-DeploymentStatus.ps1
**Verificación de estado** - Muestra información detallada sobre los recursos desplegados en Azure.

---

## Deploy-PhotosMarket.ps1

Script de PowerShell que replica la funcionalidad de los GitHub Actions para desplegar la aplicación a Azure Container Apps.

### Requisitos Previos

1. **Azure CLI** instalado y configurado
   ```powershell
   # Verificar instalación
   az --version
   
   # Instalar si es necesario
   # https://docs.microsoft.com/cli/azure/install-azure-cli
   ```

2. **Docker Desktop** instalado y corriendo
   ```powershell
   # Verificar instalación
   docker --version
   docker ps
   ```

3. **Login en Azure**
   ```powershell
   az login
   ```

### Uso Básico

#### Desplegar Backend y Frontend (Completo)
```powershell
.\Deploy-PhotosMarket.ps1
```

#### Desplegar Solo Backend
```powershell
.\Deploy-PhotosMarket.ps1 -Component Backend
```

#### Desplegar Solo Frontend
```powershell
.\Deploy-PhotosMarket.ps1 -Component Frontend
```

#### Desplegar Sin Hacer Build (Usar Imagen Existente)
```powershell
.\Deploy-PhotosMarket.ps1 -SkipBuild -ImageTag "latest"
```

### Parámetros

| Parámetro | Descripción | Valores | Default |
|-----------|-------------|---------|---------|
| `-Component` | Componente a desplegar | `Backend`, `Frontend`, `All` | `All` |
| `-ResourceGroupName` | Nombre del Resource Group | string | `rg-photosmarket-dev` |
| `-Environment` | Ambiente de despliegue | `dev`, `staging`, `prod` | `dev` |
| `-SkipBuild` | Omitir build de imágenes Docker | switch | `false` |
| `-ImageTag` | Tag personalizado para las imágenes | string | timestamp actual |

### Ejemplos Avanzados

#### Desplegar a Producción con Tag Específico
```powershell
.\Deploy-PhotosMarket.ps1 -Environment prod -ImageTag "v1.2.3"
```

#### Actualizar Solo el Frontend con Imagen Existente
```powershell
.\Deploy-PhotosMarket.ps1 -Component Frontend -SkipBuild -ImageTag "20260318120000"
```

#### Desplegar a Staging con Build Completo
```powershell
.\Deploy-PhotosMarket.ps1 -Environment staging -Component All
```

### Variables de Configuración

El script utiliza las siguientes variables basadas en los workflows de GitHub Actions:

```powershell
$config = @{
    ResourceGroup = 'rg-photosmarket-dev'
    ContainerRegistry = 'photosmarketacrdev'
    BackendImageName = 'photosmarket-backend'
    BackendContainerApp = 'photosmarket-backend-dev'
    FrontendImageName = 'photosmarket-frontend'
    FrontendContainerApp = 'photosmarket-frontend-dev'
    Region = 'eastus'
}
```

### Proceso de Despliegue

El script ejecuta los siguientes pasos:

1. **Verificaciones**
   - Verifica que Azure CLI esté instalado
   - Verifica que Docker esté instalado y corriendo (si no se usa `-SkipBuild`)
   - Verifica login en Azure

2. **Build y Push** (si no se especifica `-SkipBuild`)
   - Login a Azure Container Registry
   - Build de imágenes Docker
   - Tag de imágenes con el tag especificado y `latest`
   - Push a ACR

3. **Despliegue**
   - Obtiene URLs de los servicios existentes
   - Actualiza Container Apps con las nuevas imágenes
   - Configura variables de entorno cruzadas:
     - Backend recibe `FRONTEND_URL`
     - Frontend recibe `VITE_API_URL`
   - Verifica el despliegue

4. **Resumen**
   - Muestra URLs de los servicios desplegados
   - Muestra nombres de las revisiones

### Troubleshooting

#### Error: "Docker daemon no está corriendo"
```powershell
# Inicia Docker Desktop y espera a que esté completamente iniciado
```

#### Error: "No estás logueado en Azure"
```powershell
az login
```

#### Error al hacer push a ACR
```powershell
# Verifica permisos en ACR
az acr show --name photosmarketacrdev --query "loginServer"

# Intenta login manual
az acr login --name photosmarketacrdev
```

#### Ver logs de Container App
```powershell
# Backend logs
az containerapp logs show `
  --name photosmarket-backend-dev `
  --resource-group rg-photosmarket-dev `
  --follow

# Frontend logs
az containerapp logs show `
  --name photosmarket-frontend-dev `
  --resource-group rg-photosmarket-dev `
  --follow
```

### Comparación con GitHub Actions

Este script replica exactamente la funcionalidad de:
- `.github/workflows/deploy-backend.yml`
- `.github/workflows/deploy-frontend.yml`

Ventajas del script local:
- ✅ Despliegue desde tu máquina local
- ✅ No requiere push a GitHub
- ✅ Útil para desarrollo y testing
- ✅ Mayor control sobre el proceso

Ventajas de GitHub Actions:
- ✅ Automatización completa en CI/CD
- ✅ Triggers automáticos en push
- ✅ Historial de despliegues
- ✅ Secrets management

### Notas

- El script usa el mismo registro de contenedores y nombres de recursos que los workflows de GitHub Actions
- Los tags de imagen se generan automáticamente con timestamp si no se especifican
- Las URLs se configuran automáticamente entre frontend y backend
- El script incluye validación y manejo de errores en cada paso

---

## Update-ServiceUrls.ps1

Script rápido para actualizar las variables de entorno entre servicios sin hacer rebuild ni deployment de imágenes.

### Uso

```powershell
# Actualizar URLs en ambiente dev
.\Update-ServiceUrls.ps1

# Actualizar URLs en producción
.\Update-ServiceUrls.ps1 -Environment prod
```

### Parámetros

| Parámetro | Descripción | Default |
|-----------|-------------|---------|
| `-ResourceGroupName` | Nombre del Resource Group | `rg-photosmarket-dev` |
| `-Environment` | Ambiente (`dev`, `staging`, `prod`) | `dev` |

### ¿Cuándo usar este script?

- Después de cambios en la infraestructura que afecten las URLs
- Cuando los servicios no se comunican correctamente
- Para forzar una actualización de configuración sin redeploy completo

---

## Get-DeploymentStatus.ps1

Script para verificar el estado completo de los recursos de Photos Market en Azure.

### Uso

```powershell
# Ver estado básico
.\Get-DeploymentStatus.ps1

# Ver estado con logs recientes
.\Get-DeploymentStatus.ps1 -ShowLogs

# Ver estado en producción
.\Get-DeploymentStatus.ps1 -Environment prod -ShowLogs
```

### Parámetros

| Parámetro | Descripción | Default |
|-----------|-------------|---------|
| `-ResourceGroupName` | Nombre del Resource Group | `rg-photosmarket-dev` |
| `-Environment` | Ambiente (`dev`, `staging`, `prod`) | `dev` |
| `-ShowLogs` | Mostrar logs recientes | `false` |

### Información Mostrada

El script muestra información detallada sobre:

- ✅ Estado del Resource Group
- 🐳 Azure Container Registry y sus imágenes
- 🚀 Container Apps (Backend y Frontend):
  - Estado de aprovisionamiento
  - Revisión actual
  - URLs públicas
  - Configuración de recursos (CPU, memoria)
  - Variables de entorno
  - Health probes
  - Estado de réplicas
- 🔗 Conectividad entre servicios
- 🧪 Pruebas de endpoints
- 📜 Logs recientes (opcional)

### ¿Cuándo usar este script?

- Antes de realizar un despliegue
- Para debugging de problemas de producción
- Para verificar configuración actual
- Para obtener URLs de los servicios

---

## 🚀 Workflow Recomendado

### Despliegue Completo (Primera Vez)
```powershell
# 1. Verificar estado actual
.\Get-DeploymentStatus.ps1

# 2. Desplegar todo
.\Deploy-PhotosMarket.ps1

# 3. Verificar despliegue
.\Get-DeploymentStatus.ps1 -ShowLogs
```

### Actualización Rápida (Solo Código)
```powershell
# Desplegar solo el componente modificado
.\Deploy-PhotosMarket.ps1 -Component Backend
# o
.\Deploy-PhotosMarket.ps1 -Component Frontend
```

### Solo Actualizar Configuración
```powershell
# Sin rebuild, útil cuando solo cambió infraestructura
.\Update-ServiceUrls.ps1
```

### Debugging
```powershell
# Ver estado completo con logs
.\Get-DeploymentStatus.ps1 -ShowLogs

# Si hay problemas, redesplegar con imagen específica
.\Deploy-PhotosMarket.ps1 -Component Backend -SkipBuild -ImageTag "20260318120000"
```

---

## 🔧 Variables de Entorno en Azure

### Backend
- `FRONTEND_URL`: URL del frontend (configurada automáticamente)
- Otras variables configuradas en el Container App

### Frontend
- `VITE_API_URL`: URL del backend (configurada automáticamente)

Estas variables se configuran automáticamente durante el despliegue basándose en las URLs reales de los Container Apps.
