# ✅ Mejoras al Workflow de Infraestructura

## 📋 Resumen de Cambios

Este documento resume las mejoras realizadas al workflow `deploy-infra.yml` para hacerlo más robusto, controlable y alineado con las apps actualmente desplegadas.

---

## 🆕 Nuevas Características

### 1. Workflow Dispatch Inputs (Ejecución a Demanda)

Ahora puedes ejecutar el workflow manualmente con control total:

```yaml
inputs:
  environment: dev | staging | prod    # Seleccionar environment
  validation_only: true | false        # Solo validar sin desplegar
  force_update_oauth: true | false     # Actualizar OAuth config
```

**Ejemplo de uso:**
- Ve a: GitHub → Actions → Deploy Infrastructure → Run workflow
- Selecciona los inputs deseados
- Click "Run workflow"

---

### 2. Validación de Secrets (Job Separado)

**Job: `validate-secrets`**

Antes del deployment, verifica que todos los secrets requeridos estén configurados:

```
SECRETS REQUERIDOS:
✅ AZURE_CREDENTIALS
✅ GOOGLE_OAUTH_CLIENT_ID
✅ GOOGLE_OAUTH_CLIENT_SECRET
✅ JWT_SECRET_KEY
✅ GOOGLE_DRIVE_ROOT_FOLDER_ID
✅ GOOGLE_DRIVE_CREDENTIALS
```

**Beneficio:** Falla rápido si falta algo, antes de iniciar el deployment.

---

### 3. Validación de Bicep Pre-Deployment

**Step: `Validate Bicep template`**

Valida el template antes de desplegar:
- ✅ Verifica syntax de Bicep
- ✅ Valida parámetros
- ✅ Previene deployments con errores

```bash
az deployment group validate \
  --template-file ./infra/main.bicep \
  --parameters ./infra/main.bicepparam
```

---

### 4. Exportación Correcta de Variables de Entorno

**Antes:**
```yaml
env:
  GOOGLE_OAUTH_CLIENT_ID: ${{ secrets.GOOGLE_OAUTH_CLIENT_ID }}
  # ❌ No se usaba en el comando
```

**Ahora:**
```bash
export GOOGLE_OAUTH_CLIENT_ID="${{ secrets.GOOGLE_OAUTH_CLIENT_ID }}"
# ✅ Se exporta antes de az deployment
```

Esto permite que `readEnvironmentVariable()` en Bicep funcione correctamente.

---

### 5. Outputs Estructurados

**Step: `Get deployment outputs`**

Captura y expone outputs del deployment como variables del job:

```yaml
outputs:
  frontend-url: https://photosmarket-frontend-dev.xxx.azurecontainerapps.io
  backend-url: https://photosmarket-backend-dev.xxx.azurecontainerapps.io
  frontend-fqdn: photosmarket-frontend-dev.xxx.azurecontainerapps.io
  backend-fqdn: photosmarket-backend-dev.xxx.azurecontainerapps.io
```

Estos outputs se usan en el siguiente job.

---

### 6. Verificación Automática de OAuth

**Step: `Verify OAuth configuration`**

Después del deployment, verifica y corrige la configuración OAuth:

```bash
🔍 Verificando:
   ✅ FRONTEND_URL en Backend = https://[frontend-fqdn-real]
   ✅ 3 Scopes configurados (email, profile, openid)

🔧 Si hay mismatch:
   → Actualiza automáticamente Backend con URL correcta
```

**Beneficio:** Previene el error "Missing required parameter: scope"

---

### 7. Job de Sincronización de URLs

**Job: `update-service-urls`**

Job separado que se ejecuta después del deployment y actualiza las URLs cruzadas:

```
Frontend → Backend:
   VITE_API_URL = https://[backend-fqdn-real]

Backend → Frontend:
   FRONTEND_URL = https://[frontend-fqdn-real]
```

**Beneficio:** Frontend y Backend siempre sincronizados, sin importar qué FQDN asignó Azure.

---

### 8. Modo "Validation Only"

Ejecuta solo la validación sin hacer deployment:

```yaml
validation_only: true
```

**Cuándo usar:**
- Antes de cambios mayores en infraestructura
- Para verificar que el template sea válido
- Para testing de CI/CD sin afectar Azure

---

### 9. Resumen Visual Mejorado

Al finalizar, muestra un resumen completo:

```
╔═══════════════════════════════════════════════════════════╗
║       INFRASTRUCTURE DEPLOYMENT COMPLETED                 ║
╚═══════════════════════════════════════════════════════════╝

🌐 Application URLs:
   Frontend: https://photosmarket-frontend-dev.xxx.azurecontainerapps.io
   Backend:  https://photosmarket-backend-dev.xxx.azurecontainerapps.io

📦 Resources:
   Name                           FQDN                                    Status
   photosmarket-frontend-dev      xxx.azurecontainerapps.io              Running
   photosmarket-backend-dev       xxx.azurecontainerapps.io              Running

🔐 OAuth Configuration:
   Frontend FQDN: photosmarket-frontend-dev.xxx.azurecontainerapps.io
   Redirect URI:  https://photosmarket-frontend-dev.xxx.azurecontainerapps.io/callback

⚠️  IMPORTANT: Configure this redirect URI in Google Cloud Console
```

---

## 📊 Flujo Completo

```
┌─────────────────────────────────────────────────────────────┐
│  TRIGGER: Push to main (infra/) o Manual Dispatch          │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────┐
│  JOB 1: validate-secrets                                    │
│  ✅ Verifica que todos los secrets existan                  │
│  ❌ Falla si falta alguno (con lista de faltantes)          │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────┐
│  JOB 2: deploy-infrastructure (needs: validate-secrets)    │
│  1. Checkout código                                         │
│  2. Login Azure                                             │
│  3. Create/Verify Resource Group                            │
│  4. Validate Bicep template ✨                              │
│  5. [Si validation_only=false]                              │
│     ├─ Deploy Bicep infrastructure                          │
│     ├─ Capturar outputs (URLs/FQDNs) ✨                     │
│     ├─ Mostrar resumen visual ✨                            │
│     └─ Verificar OAuth config ✨                            │
│  6. [Si validation_only=true]                               │
│     └─ Mostrar resumen de validación                        │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────┐
│  JOB 3: update-service-urls (needs: deploy-infrastructure) │
│  Solo si validation_only=false                              │
│  1. Login Azure                                             │
│  2. Update Frontend → VITE_API_URL = backend-fqdn ✨        │
│  3. Update Backend → FRONTEND_URL = frontend-fqdn ✨        │
│  4. Mostrar configuración final                             │
└─────────────────────────────────────────────────────────────┘
```

---

## 🎯 Casos de Uso

### Caso 1: Primera Vez (Setup Inicial)

**Objetivo:** Desplegar toda la infraestructura desde cero

**Pasos:**
1. Configura todos los secrets en GitHub
2. Ve a: Actions → Deploy Infrastructure → Run workflow
3. Inputs:
   - `environment`: dev
   - `validation_only`: false
   - `force_update_oauth`: true
4. Run workflow
5. Espera ~10-15 minutos
6. Copia el redirect URI del output
7. Configúralo en Google Cloud Console

---

### Caso 2: Validar Cambios Antes de Desplegar

**Objetivo:** Verificar que cambios en Bicep sean válidos sin afectar Azure

**Pasos:**
1. Edita archivos en `infra/`
2. Ve a: Actions → Deploy Infrastructure → Run workflow
3. Inputs:
   - `environment`: dev
   - `validation_only`: **true** ← Solo valida
4. Run workflow
5. Si pasa: vuelve a ejecutar con `validation_only: false`
6. Si falla: corrige errores y vuelve al paso 2

---

### Caso 3: Actualizar Infraestructura (Automático)

**Objetivo:** Cambios en infraestructura despliegan automáticamente

**Pasos:**
1. Edita archivos en `infra/`
   ```bash
   vim infra/modules/backend-container-app.bicep
   ```
2. Commit y push a main
   ```bash
   git add infra/
   git commit -m "feat: Update backend resources"
   git push origin main
   ```
3. El workflow se ejecuta automáticamente
4. Monitorea en GitHub Actions

---

### Caso 4: Solo Actualizar OAuth Config

**Objetivo:** Corregir configuración OAuth sin redesplegar infraestructura

**Pasos:**
1. Ve a: Actions → Deploy Infrastructure → Run workflow
2. Inputs:
   - `environment`: dev
   - `validation_only`: false
   - `force_update_oauth`: **true** ← Fuerza actualización
3. Run workflow
4. El workflow verifica y actualiza FRONTEND_URL si es necesario

---

### Caso 5: Desplegar a Staging/Prod

**Objetivo:** Promocionar infraestructura a otro environment

**Pasos:**
1. Ve a: Actions → Deploy Infrastructure → Run workflow
2. Inputs:
   - `environment`: **staging** o **prod**
   - `validation_only`: false
   - `force_update_oauth`: true
3. Run workflow
4. Recursos se crean/actualizan en ese environment

---

## 🔍 Verificación Post-Deployment

### Checklist Rápido

```bash
# ✅ 1. Verificar que Container Apps existan
az containerapp list \
  --resource-group rg-photosmarket-dev \
  -o table

# ✅ 2. Verificar secrets en Key Vault
az keyvault secret list \
  --vault-name photosmarket-kv-dev \
  -o table

# ✅ 3. Verificar OAuth config
cd scripts
.\Fix-AzureOAuthConfig.ps1 -ResourceGroupName rg-photosmarket-dev

# ✅ 4. Probar las aplicaciones
# Abre el Frontend URL y haz login
```

---

## ⚠️ Cambios Importantes

### Variables de Entorno Ahora se Exportan Correctamente

**Antes:**
```yaml
env:
  GOOGLE_OAUTH_CLIENT_ID: ${{ secrets.GOOGLE_OAUTH_CLIENT_ID }}
run: |
  az deployment ...
  # ❌ bicepparam no podía leer las env vars
```

**Ahora:**
```yaml
run: |
  export GOOGLE_OAUTH_CLIENT_ID="${{ secrets.GOOGLE_OAUTH_CLIENT_ID }}"
  az deployment ...
  # ✅ bicepparam lee correctamente con readEnvironmentVariable()
```

### Jobs Separados para Mejor Control

**Antes:**
- 1 job que hacía todo
- Sin validación de secrets
- Sin verificación OAuth

**Ahora:**
- Job 1: Validar secrets
- Job 2: Desplegar infraestructura y verificar OAuth
- Job 3: Sincronizar URLs entre servicios

### Outputs Estructurados

Los outputs del deployment ahora están disponibles como variables del job:

```yaml
outputs:
  frontend-url: ${{ steps.get-outputs.outputs.frontend-url }}
  backend-url: ${{ steps.get-outputs.outputs.backend-url }}
  frontend-fqdn: ${{ steps.get-outputs.outputs.frontend-fqdn }}
  backend-fqdn: ${{ steps.get-outputs.outputs.backend-fqdn }}
```

Esto permite que el siguiente job los use.

---

## 📚 Documentación Adicional

| Documento | Descripción |
|-----------|-------------|
| [DEPLOY-INFRA-WORKFLOW.md](DEPLOY-INFRA-WORKFLOW.md) | Guía completa del workflow |
| [DEPLOYMENT-FLOW.md](DEPLOYMENT-FLOW.md) | Flujo de deployment de apps |
| [TROUBLESHOOTING-OAUTH.md](TROUBLESHOOTING-OAUTH.md) | Solución de errores OAuth |

---

## 🎉 Resultado Final

Con estas mejoras:

✅ El workflow de infraestructura es **robusto** y **controlable**
✅ Valida todo antes de desplegar
✅ Sincroniza URLs automáticamente
✅ Previene errores de OAuth
✅ Proporciona outputs claros
✅ Permite ejecución a demanda con flags
✅ Soporta múltiples environments
✅ Incluye modo validation-only

**El workflow ahora tiene todo lo necesario para que las apps funcionen correctamente, sin importar qué FQDNs asigne Azure.**

---

**Siguiente paso:** Commit y push de estos cambios:

```bash
git add .github/workflows/deploy-infra.yml docs/DEPLOY-INFRA-WORKFLOW.md
git commit -m "feat: Improve infrastructure deployment workflow

- Add workflow dispatch inputs for control
- Add secrets validation job
- Add Bicep validation before deployment
- Fix environment variable export for bicepparam
- Add OAuth verification and auto-correction
- Add URL synchronization job
- Improve outputs and summaries
- Add validation-only mode
- Add comprehensive documentation"
git push origin main
```
