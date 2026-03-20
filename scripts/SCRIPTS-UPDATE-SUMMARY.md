# Actualización de Scripts de Despliegue - Photos Market

## 📋 Resumen de Cambios

Se han actualizado los scripts de despliegue manual para ser consistentes con las correcciones aplicadas en los workflows de GitHub Actions, específicamente para resolver el error **"Missing required parameter: scope"** de OAuth.

## 🆕 Scripts Nuevos

### 1. Deploy-Infrastructure.ps1 ⭐
**Ubicación:** `scripts/Deploy-Infrastructure.ps1`

**Propósito:** Despliegue de infraestructura completa en Azure

**Características:**
- ✅ Validación de Azure CLI y autenticación
- ✅ Verificación de archivos Bicep
- ✅ Validación de Bicep template antes de desplegar
- ✅ Advertencia sobre secretos requeridos (con lista de verificación)
- ✅ Verificación automática de OAuth después del deployment
- ✅ Corrección automática de FRONTEND_URL si no coincide
- ✅ Modo de validación sin despliegue (`-ValidationOnly`)
- ✅ Resumen detallado con URLs y próximos pasos

**Uso:**
```powershell
# Desplegar infraestructura completa
.\Deploy-Infrastructure.ps1

# Solo validar (sin desplegar)
.\Deploy-Infrastructure.ps1 -ValidationOnly

# Desplegar en producción
.\Deploy-Infrastructure.ps1 -Environment prod -Location westus
```

**Reemplaza a:** `deploy-azure.ps1` (ahora deprecado)

---

## 🔄 Scripts Actualizados

### 2. Deploy-PhotosMarket.ps1 ✅
**Ubicación:** `scripts/Deploy-PhotosMarket.ps1`

**Cambios Principales:**

#### ⚠️ CORRECCIÓN CRÍTICA: Orden de Despliegue
**ANTES:** Desplegaba Backend y Frontend sin orden específico
**AHORA:** Despliega **Frontend PRIMERO, luego Backend**

**Por qué es importante:** El Backend necesita el FQDN real del Frontend para configurar FRONTEND_URL correctamente. Si se despliega en el orden incorrecto, FRONTEND_URL será incorrecta y causará el error OAuth.

#### Nuevas Funcionalidades:
1. **Deploy Frontend Primero**
   - Obtiene Backend FQDN actual (si existe) para configurar VITE_API_URL
   - Despliega Frontend
   - Captura el Frontend FQDN real después del deployment

2. **Deploy Backend con Frontend FQDN Real**
   - Usa el Frontend FQDN real obtenido en el paso anterior
   - Configura FRONTEND_URL correctamente
   - Evita el error "Missing required parameter: scope"

3. **Verificación OAuth Automática**
   - Verifica FRONTEND_URL después del deployment
   - Verifica que haya 3 OAuth scopes configurados
   - Muestra advertencias si hay problemas

4. **Sincronización de URLs**
   - Actualiza Frontend con Backend URL final
   - Asegura que ambos servicios tengan las URLs correctas

5. **Nuevos Parámetros:**
   - `-SkipOAuthVerification`: Omite la verificación OAuth
   - Cambiado: `'All'` → `'Both'` para el parámetro Component

**Código Clave (Funciones Actualizadas):**

```powershell
# La función Deploy-Backend ahora REQUIERE Frontend FQDN
function Deploy-Backend {
    param(
        ...
        [string]$FrontendFqdn  # NUEVO: Requerido
    )
    
    # Usa el Frontend FQDN proporcionado (real)
    $frontendUrl = "https://$FrontendFqdn"
    $envVars = @("FRONTEND_URL=$frontendUrl")
    
    az containerapp update `
        --name $ContainerApp `
        --set-env-vars $envVars
    ...
}

# La función Deploy-Frontend recibe Backend FQDN opcional
function Deploy-Frontend {
    param(
        ...
        [string]$BackendFqdn  # NUEVO: Opcional
    )
    
    if ($BackendFqdn) {
        $backendUrl = "https://$BackendFqdn"
        az containerapp update `
            --set-env-vars "VITE_API_URL=$backendUrl"
    }
    ...
}
```

**Flujo de Despliegue (SCRIPT PRINCIPAL):**

```powershell
# PASO 1: Deploy Frontend primero
if ($Component -eq 'Frontend' -or $Component -eq 'Both') {
    # Obtener Backend FQDN actual (si existe)
    $backendFqdn = az containerapp show ... 
    
    # Deploy Frontend
    Deploy-Frontend ... -BackendFqdn $backendFqdn
    
    # Obtener Frontend FQDN real
    $frontendFqdn = az containerapp show ...
}

# PASO 2: Deploy Backend con Frontend FQDN real
if ($Component -eq 'Backend' -or $Component -eq 'Both') {
    # Obtener Frontend FQDN si no lo tenemos
    if (-not $frontendFqdn) {
        $frontendFqdn = az containerapp show ...
    }
    
    # Deploy Backend con FQDN real
    Deploy-Backend ... -FrontendFqdn $frontendFqdn
}

# PASO 3: Verificar OAuth Configuration
if (-not $SkipOAuthVerification) {
    # Verificar FRONTEND_URL configurado
    # Verificar OAuth Scopes
    # Mostrar advertencias si hay problemas
}

# PASO 4: Sincronizar URLs
if ($Component -eq 'Both') {
    # Actualizar Frontend con Backend URL final
    az containerapp update ...
}
```

**Uso:**
```powershell
# Desplegar ambos (Frontend primero, luego Backend)
.\Deploy-PhotosMarket.ps1

# Desplegar solo Backend (usa Frontend FQDN existente)
.\Deploy-PhotosMarket.ps1 -Component Backend

# Desplegar solo Frontend
.\Deploy-PhotosMarket.ps1 -Component Frontend

# Desplegar sin verificar OAuth
.\Deploy-PhotosMarket.ps1 -SkipOAuthVerification
```

---

### 3. deploy-azure.ps1 ⚠️
**Estado:** DEPRECADO

**Cambios:**
- Agregado mensaje de advertencia al inicio del script
- Redirecciona a usuarios al nuevo `Deploy-Infrastructure.ps1`
- Pide confirmación antes de continuar

**Mensaje de Deprecación:**
```
╔═══════════════════════════════════════════════════════════╗
║                    ⚠️  ADVERTENCIA                        ║
╚═══════════════════════════════════════════════════════════╝

Este script (deploy-azure.ps1) está DEPRECADO.

Por favor, usa el nuevo script:
  .\Deploy-Infrastructure.ps1

El nuevo script incluye:
  • Validación de Bicep antes de desplegar
  • Verificación automática de OAuth
  • Corrección de FRONTEND_URL
  • Mejor manejo de secretos
```

---

## 📖 Documentación Actualizada

### 4. scripts/README.md ✅

**Actualizaciones:**
- ✅ Agregado flujo de despliegue recomendado
- ✅ Documentación completa de Deploy-Infrastructure.ps1
- ✅ Documentación actualizada de Deploy-PhotosMarket.ps1
- ✅ Documentación completa de Fix-AzureOAuthConfig.ps1
- ✅ Sección de parámetros actualizada con nuevo componente 'Both'
- ✅ Advertencia sobre el orden de despliegue correcto

---

## 🔍 Validación de Consistencia

### Comparación con Workflows de GitHub Actions

| Aspecto | deploy-to-azure.yml | Deploy-PhotosMarket.ps1 | ✅ |
|---------|---------------------|-------------------------|-----|
| Orden de despliegue | Frontend → Backend | Frontend → Backend | ✅ |
| FRONTEND_URL | Usa FQDN real | Usa FQDN real | ✅ |
| OAuth verification | Sí | Sí (opcional) | ✅ |
| URL synchronization | Sí | Sí | ✅ |
| Build images | Sí | Sí (opcional) | ✅ |
| Tag management | Sí | Sí | ✅ |

| Aspecto | deploy-infra.yml | Deploy-Infrastructure.ps1 | ✅ |
|---------|------------------|---------------------------|-----|
| Validate secrets | Sí | Sí (advertencia) | ✅ |
| Validate Bicep | Sí | Sí | ✅ |
| OAuth verification | Sí | Sí (opcional) | ✅ |
| FRONTEND_URL correction | Sí | Sí | ✅ |
| Validation mode | Sí | Sí (`-ValidationOnly`) | ✅ |

---

## 🚀 Flujo de Uso Recomendado

### Primera Vez (Despliegue Completo)

```powershell
# 1. Validar infraestructura
.\Deploy-Infrastructure.ps1 -ValidationOnly

# 2. Desplegar infraestructura
.\Deploy-Infrastructure.ps1

# 3. Desplegar aplicaciones (Frontend → Backend)
.\Deploy-PhotosMarket.ps1

# 4. Verificar OAuth (si es necesario)
.\Fix-AzureOAuthConfig.ps1 -ResourceGroupName "rg-photosmarket-dev"
```

### Actualizar Solo Aplicaciones

```powershell
# Opción 1: Ambas aplicaciones (recomendado)
.\Deploy-PhotosMarket.ps1

# Opción 2: Solo Backend
.\Deploy-PhotosMarket.ps1 -Component Backend

# Opción 3: Solo Frontend
.\Deploy-PhotosMarket.ps1 -Component Frontend
```

### Corregir URLs Desincronizadas

```powershell
# Sincronizar URLs entre Frontend y Backend
.\Update-ServiceUrls.ps1 -ResourceGroupName "rg-photosmarket-dev"
```

### Corregir Problemas de OAuth

```powershell
# Diagnosticar y corregir OAuth
.\Fix-AzureOAuthConfig.ps1 -ResourceGroupName "rg-photosmarket-dev"
```

---

## ✅ Validación de No Regresión

### Funcionalidad Preservada

✅ **Build de imágenes Docker**
- Sigue funcionando igual
- Usa mismo ACR
- Mismo sistema de tags

✅ **Push a ACR**
- Mismo proceso
- Login automático
- Tags con timestamp y latest

✅ **Despliegue a Container Apps**
- Mismos nombres de recursos
- Mismas configuraciones
- Mismas variables de entorno

✅ **Configuración de URLs**
- Frontend recibe VITE_API_URL
- Backend recibe FRONTEND_URL
- URLs sincronizadas

### Mejoras sin Romper Funcionalidad

✅ **Orden de despliegue correcto**
- Mejora: Frontend primero, Backend después
- No rompe: Ambos se despliegan correctamente
- Beneficio: FRONTEND_URL siempre correcta

✅ **Verificación OAuth**
- Mejora: Verificación automática después del deployment
- No rompe: Es opcional (`-SkipOAuthVerification`)
- Beneficio: Detecta problemas inmediatamente

✅ **Validación de infraestructura**
- Mejora: Valida Bicep antes de desplegar
- No rompe: Falla antes si hay errores
- Beneficio: Evita deployments fallidos

---

## 📝 Notas Importantes

### Para Usuarios Existentes

1. **Si usabas `deploy-azure.ps1`:**
   - Cambia a `Deploy-Infrastructure.ps1`
   - El nuevo script incluye todas las funcionalidades y más

2. **Si usabas `Deploy-PhotosMarket.ps1`:**
   - El script sigue funcionando
   - Ahora despliega en el orden correcto automáticamente
   - Parámetro `Component` acepta 'Both' en lugar de 'All'

3. **Si tenías problemas de OAuth:**
   - Los nuevos scripts incluyen verificación automática
   - Usa `Fix-AzureOAuthConfig.ps1` para corregir problemas existentes

### Requisitos

- Azure CLI instalado y autenticado
- Docker Desktop (para build de imágenes)
- Permisos de Contributor en Azure
- Archivos Bicep en `infra/`
- Secretos configurados (ver Deploy-Infrastructure.ps1)

### Compatibilidad

✅ Windows PowerShell 5.1+
✅ PowerShell Core 7.0+
❌ Bash/Shell scripts (usa GitHub Actions workflows)

---

## 🐛 Troubleshooting

### Error: "Frontend FQDN no proporcionado"

**Causa:** Intentando desplegar Backend sin Frontend existente

**Solución:**
```powershell
# Opción 1: Desplegar ambos
.\Deploy-PhotosMarket.ps1 -Component Both

# Opción 2: Desplegar Frontend primero, luego Backend
.\Deploy-PhotosMarket.ps1 -Component Frontend
.\Deploy-PhotosMarket.ps1 -Component Backend
```

### Error: "Missing required parameter: scope"

**Causa:** OAuth scopes no configurados o FRONTEND_URL incorrecta

**Solución:**
```powershell
.\Fix-AzureOAuthConfig.ps1 -ResourceGroupName "rg-photosmarket-dev"
```

### URLs Desincronizadas

**Solución:**
```powershell
.\Update-ServiceUrls.ps1 -ResourceGroupName "rg-photosmarket-dev"
```

---

## 📚 Recursos Adicionales

- **Documentación completa:** `scripts/README.md`
- **Troubleshooting OAuth:** `docs/TROUBLESHOOTING-OAUTH.md`
- **GitHub Actions workflows:** `.github/workflows/`

---

## ✨ Resumen

Los scripts de despliegue manual ahora están completamente sincronizados con los workflows de GitHub Actions y incluyen todas las correcciones necesarias para resolver el error de OAuth "Missing required parameter: scope".

**Archivos modificados:**
- ✅ `scripts/Deploy-Infrastructure.ps1` (NUEVO)
- ✅ `scripts/Deploy-PhotosMarket.ps1` (ACTUALIZADO)
- ⚠️ `scripts/deploy-azure.ps1` (DEPRECADO)
- ✅ `scripts/README.md` (ACTUALIZADO)

**Archivos sin cambios (ya correctos):**
- ✅ `scripts/Fix-AzureOAuthConfig.ps1`
- ✅ `scripts/Update-ServiceUrls.ps1`
- ✅ `scripts/Get-DeploymentStatus.ps1`
