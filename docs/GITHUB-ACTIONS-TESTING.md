# Actualización de GitHub Actions - Integración de Pruebas Unitarias

## 📋 Resumen de Cambios

Se han actualizado los workflows de GitHub Actions para integrar las pruebas unitarias y garantizar que **ningún código con errores llegue a producción**.

---

## ✅ Cambios Realizados

### 1. Workflow CI (`ci.yml`)

**Antes:**
- ✅ Ejecutaba pruebas del Backend
- ❌ **NO** ejecutaba pruebas del Frontend

**Ahora:**
- ✅ Ejecuta pruebas del Backend (xUnit)
- ✅ **Ejecuta pruebas del Frontend (Vitest)** ← NUEVO

**Cambio específico:**
```yaml
# Agregado al job build-frontend
- name: Run Tests
  working-directory: ./src/frontend
  run: npm test
```

**Cuándo se ejecuta:**
- En **Pull Requests** hacia `main`
- En **pushes** a cualquier branch (excepto `main`)

**Propósito:**
- Validar código antes de merge
- Feedback rápido en PRs
- Prevenir bugs en código antes de llegar a main

---

### 2. Workflow Deploy (`deploy-to-azure.yml`) 

**Antes:**
- ❌ **NO** ejecutaba pruebas
- Desplegaba directamente a Azure sin validación
- Riesgo de desplegar código con bugs

**Ahora:**
- ✅ **Nuevo Job: `run-tests`** que ejecuta ANTES del deployment
- ✅ Ejecuta pruebas del Backend
- ✅ Ejecuta pruebas del Frontend
- 🔒 **Bloquea deployment** si las pruebas fallan

**Cambio específico:**
```yaml
jobs:
  # Job 0: Run Tests (blocks deployment if tests fail)
  run-tests:
    name: Run Unit Tests
    runs-on: ubuntu-latest
    
    steps:
      # Setup + Backend Tests
      - name: Run Backend Tests
        run: |
          dotnet test PhotosMarket.API.Tests/PhotosMarket.API.Tests.csproj
          if [ $? -ne 0 ]; then
            echo "❌ Backend tests failed! Deployment cancelled."
            exit 1
          fi
      
      # Setup + Frontend Tests
      - name: Run Frontend Tests
        run: |
          npm test
          if [ $? -ne 0 ]; then
            echo "❌ Frontend tests failed! Deployment cancelled."
            exit 1
          fi

  # Job 1: Deploy Frontend (depends on run-tests)
  deploy-frontend:
    needs: [run-tests]  # ← NUEVO: Requiere que las pruebas pasen
    ...

  # Job 2: Deploy Backend (depends on run-tests and frontend)
  deploy-backend:
    needs: [run-tests, deploy-frontend]  # ← NUEVO
    if: needs.run-tests.result == 'success'  # ← NUEVO
    ...
```

**Cuándo se ejecuta:**
- En **pushes a `main`**
- **Manualmente** con workflow_dispatch

**Flujo completo:**
```
┌─────────────────────────────────────────────────────────┐
│ 1. Git Push to main                                     │
└─────────────────────────────────────────────────────────┘
                       ↓
┌─────────────────────────────────────────────────────────┐
│ 2. Job: run-tests                                       │
│    ├─ Setup .NET + Restore                              │
│    ├─ Build Backend                                     │
│    ├─ Run Backend Tests (xUnit)                         │
│    │                                                     │
│    ├─ Setup Node.js                                     │
│    ├─ Install Frontend deps                             │
│    └─ Run Frontend Tests (Vitest)                       │
└─────────────────────────────────────────────────────────┘
                       ↓
              ┌────────┴────────┐
              │                 │
          ✅ Pass          ❌ Fail
              │                 │
              ↓                 ↓
┌─────────────────────┐  ┌──────────────────┐
│ 3. deploy-frontend  │  │ STOP DEPLOYMENT  │
│ 4. deploy-backend   │  │ Show error logs  │
└─────────────────────┘  └──────────────────┘
```

---

## 🔒 Protecciones Implementadas

### Backend Tests
- **Framework**: xUnit + Moq + FluentAssertions
- **Cobertura**: 19 tests
- **Componentes**: AuthService, OrderService, AuthController
- **Validaciones críticas**:
  - ✅ Validación de RUT chileno
  - ✅ Generación de JWT tokens
  - ✅ Cálculos de precios y descuentos
  - ✅ Creación de órdenes

### Frontend Tests
- **Framework**: Vitest + Vue Test Utils
- **Cobertura**: 23 tests
- **Componentes**: Cart Store, Auth Store, Auth Service
- **Validaciones críticas**:
  - ✅ Lógica del carrito de compras
  - ✅ Cálculo de totales y descuentos
  - ✅ Autenticación de usuarios
  - ✅ Persistencia en localStorage

---

## 📊 Impacto en el Workflow

### Antes
```
main ← push → Build Docker → Deploy to Azure
                                  ↓
                            ❌ Posibles bugs en prod
```

### Ahora
```
main ← push → Run Tests → Build Docker → Deploy to Azure
                  ↓                              ↓
              ✅ Pass                      ✅ Código validado
              ❌ Fail (STOP)
```

---

## 🎯 Beneficios

1. **Calidad de Código**
   - Solo código validado llega a producción
   - Feedback inmediato en PRs

2. **Prevención de Bugs**
   - Detecta errores antes del deployment
   - Evita downtime en producción

3. **Confianza en Deployments**
   - Despliegues automáticos más seguros
   - Menos rollbacks por errores

4. **Documentación Viva**
   - Tests documentan funcionalidad esperada
   - Facilita onboarding de nuevos desarrolladores

---

## 📝 Ejemplo de Ejecución

### ✅ Caso Exitoso

```bash
# GitHub Actions Log

Run Backend Tests
  Running tests in PhotosMarket.API.Tests
  ✓ ValidateRut_VariousInputs_ReturnsExpectedResult (5 ms)
  ✓ GenerateJwtToken_ValidUser_ReturnsValidToken (12 ms)
  ✓ CreateOrderAsync_ValidRequest_CreatesOrderSuccessfully (8 ms)
  ... (16 more tests)
  
  Test Run Successful.
  Total tests: 19
  Passed: 19 ✅

Run Frontend Tests
  Running tests with Vitest
  ✓ src/stores/cart.spec.js (9 tests)
  ✓ src/stores/auth.spec.js (8 tests)
  ✓ src/services/authService.spec.js (6 tests)
  
  Test Files  3 passed (3)
  Tests  23 passed (23)
  
╔══════════════════════════════════════════════════════════╗
║      ✅ All Tests Passed! Proceeding with deployment...   ║
╚══════════════════════════════════════════════════════════╝

Deploying Frontend to Azure...
✅ Frontend deployed successfully

Deploying Backend to Azure...
✅ Backend deployed successfully

Deployment completed! 🚀
```

### ❌ Caso con Error

```bash
# GitHub Actions Log

Run Backend Tests
  Running tests in PhotosMarket.API.Tests
  ✓ ValidateRut_VariousInputs_ReturnsExpectedResult (5 ms)
  ✓ GenerateJwtToken_ValidUser_ReturnsValidToken (12 ms)
  ✗ CreateOrderAsync_ValidRequest_CreatesOrderSuccessfully (FAILED)
    Expected: 15000
    Actual: 10000
  
  Test Run Failed.
  Total tests: 19
  Passed: 18
  Failed: 1 ❌

❌ Backend tests failed! Deployment cancelled.

Error: Process completed with exit code 1.

🚫 Deployment has been BLOCKED
```

---

## 🛠️ Configuración Local vs GitHub Actions

### Local Development
```powershell
# Ejecutar pruebas manualmente
.\scripts\Run-Tests.ps1

# Desplegar con pruebas (local)
.\scripts\Deploy-PhotosMarket.ps1
```

### GitHub Actions
```bash
# Automático en cada push
git push origin main

# Las pruebas se ejecutan automáticamente
# No requiere intervención manual
```

---

## 🔍 Monitoreo

### Ver resultados en GitHub

1. Ir a repositorio en GitHub
2. Click en **Actions** tab
3. Ver workflows ejecutados:
   - 🟢 Green checkmark = Tests passed, deployed
   - 🔴 Red X = Tests failed, deployment blocked
   - 🟡 Orange dot = Running

4. Click en un workflow run para ver detalles
5. Expandir job `run-tests` para ver resultados

### Notificaciones

GitHub automáticamente notifica:
- ✅ Éxito en deployments
- ❌ Fallos en tests
- 📧 Por email (configurable)
- 💬 En PRs (checks)

---

## 📚 Archivos Modificados

### `.github/workflows/ci.yml`
- Agregado: Step de pruebas del frontend

### `.github/workflows/deploy-to-azure.yml`
- Agregado: Job completo `run-tests`
- Modificado: `deploy-frontend` ahora depende de `run-tests`
- Modificado: `deploy-backend` ahora depende de `run-tests`

---

## ✅ Checklist de Validación

Antes de hacer push a `main`:

- [ ] Pruebas pasan localmente: `.\scripts\Run-Tests.ps1`
- [ ] Build funciona: `dotnet build` y `npm run build`
- [ ] No hay errores de lint: `npm run lint`
- [ ] Código commiteado y pusheado
- [ ] Verificar GitHub Actions pasa el workflow CI en PR
- [ ] Merge a main → Deployment automático con tests

---

## 🎓 Mejores Prácticas

1. **Nunca hacer force push a main**
   - Los tests protegen, no los omitas

2. **Revisar logs de GitHub Actions**
   - Si falla, lee el error completo
   - No hacer retry ciego

3. **Actualizar tests con nuevas features**
   - Cada nueva función debe tener tests
   - Mantén cobertura >80%

4. **Usar workflow_dispatch con cuidado**
   - Manual deploy solo en emergencias
   - Siempre con -SkipTests=false

5. **Confiar en el proceso**
   - Los tests están para ayudar
   - Si fallan, hay un problema real

---

## 🆘 Solución de Problemas

### Tests pasan localmente pero fallan en GitHub Actions

**Causa común:**
- Diferencias en environment (Linux vs Windows)
- Diferencias en versiones de dependencias

**Solución:**
```bash
# Verificar versiones
dotnet --version  # ¿Coincide con GitHub Actions?
node --version    # ¿Coincide con GitHub Actions?

# Ver el error exacto en GitHub Actions logs
# Reproducir localmente si es posible
```

### Deployment bloqueado por tests que "deberían pasar"

**NO hacer:**
- ❌ Deshabilitar los tests
- ❌ Modificar workflow para omitir checks

**Hacer:**
1. ✅ Leer logs completos de GitHub Actions
2. ✅ Reproducir error localmente
3. ✅ Arreglar el código o el test
4. ✅ Verificar localmente: `.\scripts\Run-Tests.ps1`
5. ✅ Push fix → Tests pasan → Deploy automático

---

## 🚀 Próximos Pasos

Posibles mejoras futuras:

- [ ] Agregar coverage reports a GitHub
- [ ] Badges de status en README
- [ ] Tests de integración (E2E)
- [ ] Performance benchmarks
- [ ] Security scanning (SAST)
- [ ] Dependabot alerts y auto-updates

---

**Fecha de actualización:** 30 de Marzo, 2026
**Autor:** GitHub Copilot (Claude Sonnet 4.5)
