# Guía de Pruebas Unitarias - Photos Market

## 📋 Tabla de Contenidos
- [Descripción General](#descripción-general)
- [Configuración Inicial](#configuración-inicial)
- [Backend - Pruebas .NET](#backend---pruebas-net)
- [Frontend - Pruebas Vue.js](#frontend---pruebas-vuejs)
- [Ejecutar Pruebas](#ejecutar-pruebas)
- [Integración con CI/CD](#integración-con-cicd)
- [Mejores Prácticas](#mejores-prácticas)

---

## 📖 Descripción General

Este proyecto incluye pruebas unitarias completas para garantizar la calidad y estabilidad del código tanto en el Backend (.NET/C#) como en el Frontend (Vue.js).

### ✅ Beneficios de las Pruebas
- **Prevención de errores**: Detecta bugs antes de llegar a producción
- **Documentación viva**: Las pruebas documentan cómo funciona el código
- **Refactorización segura**: Permite cambiar código con confianza
- **Integración continua**: Bloquea despliegues con errores

### 🎯 Cobertura de Pruebas

#### Backend
- ✅ **AuthService**: Autenticación, validación de RUT, generación de tokens JWT
- ✅ **OrderService**: Creación de órdenes, cálculo de precios, descuentos
- ✅ **AuthController**: Endpoints de autenticación, manejo de errores

#### Frontend
- ✅ **Cart Store**: Gestión del carrito, cálculo de totales, descuentos por volumen
- ✅ **Auth Store**: Autenticación de usuarios, gestión de tokens
- ✅ **Auth Service**: Comunicación con API de autenticación

---

## 🛠️ Configuración Inicial

### Prerrequisitos

**Backend:**
```powershell
# .NET SDK 8.0 o superior
dotnet --version
```

**Frontend:**
```powershell
# Node.js 18+ y npm
node --version
npm --version
```

### Instalación de Dependencias

**Backend:**
```powershell
cd src/backend/PhotosMarket.API.Tests
dotnet restore
```

**Frontend:**
```powershell
cd src/frontend
npm install
```

---

## 🔧 Backend - Pruebas .NET

### Framework y Herramientas
- **xUnit**: Framework de pruebas
- **Moq**: Biblioteca para crear mocks
- **FluentAssertions**: Assertions más legibles

### Estructura del Proyecto
```
src/backend/PhotosMarket.API.Tests/
├── PhotosMarket.API.Tests.csproj
├── Services/
│   ├── AuthServiceTests.cs
│   └── OrderServiceTests.cs
└── Controllers/
    └── AuthControllerTests.cs
```

### Ejecutar Pruebas Backend

**Ejecutar todas las pruebas:**
```powershell
cd src/backend
dotnet test PhotosMarket.API.Tests/PhotosMarket.API.Tests.csproj
```

**Con reporte verbose:**
```powershell
dotnet test --verbosity detailed
```

**Con cobertura de código:**
```powershell
dotnet test --collect:"XPlat Code Coverage"
```

### Ejemplos de Pruebas Backend

#### AuthService - Validación de RUT
```csharp
[Theory]
[InlineData("12345678-9", true)]
[InlineData("11111111-1", true)]
[InlineData("12345678-0", false)]
[InlineData("invalid", false)]
public void ValidateRut_VariousInputs_ReturnsExpectedResult(string rut, bool expectedResult)
{
    var result = _authService.ValidateRut(rut);
    result.Should().Be(expectedResult);
}
```

#### OrderService - Cálculo de Descuentos
```csharp
[Fact]
public async Task CreateOrderAsync_WithBulkDiscount_AppliesDiscountCorrectly()
{
    // Arrange: 10 photos * 5000 = 50000, 15% discount = 7500, Final: 42500
    
    // Act
    var result = await _orderService.CreateOrderAsync(userId, userEmail, userName, photos);
    
    // Assert
    result.Subtotal.Should().Be(50000);
    result.DiscountPercentage.Should().Be(15);
    result.DiscountAmount.Should().Be(7500);
    result.TotalAmount.Should().Be(42500);
}
```

---

## 🎨 Frontend - Pruebas Vue.js

### Framework y Herramientas
- **Vitest**: Framework de pruebas rápido para Vite
- **Vue Test Utils**: Utilidades oficiales para probar componentes Vue
- **Happy DOM**: Entorno DOM para pruebas

### Estructura del Proyecto
```
src/frontend/src/
├── vitest.config.js
├── stores/
│   ├── cart.spec.js
│   └── auth.spec.js
└── services/
    └── authService.spec.js
```

### Ejecutar Pruebas Frontend

**Ejecutar todas las pruebas:**
```powershell
cd src/frontend
npm test
```

**Modo watch (desarrollo):**
```powershell
npm run test:watch
```

**Con cobertura de código:**
```powershell
npm run test:coverage
```

### Ejemplos de Pruebas Frontend

#### Cart Store - Descuentos por Volumen
```javascript
it('should apply bulk discount when threshold is met', () => {
  const cart = useCartStore()
  cart.pricePerPhoto = 5000
  cart.bulkDiscountMinPhotos = 5
  cart.bulkDiscountPercentage = 20
  
  // Add 5 photos to trigger discount
  for (let i = 1; i <= 5; i++) {
    cart.addItem({ id: `photo${i}`, ... })
  }
  
  const expectedTotal = 25000 - (25000 * 0.20) // 20000
  expect(cart.totalAmount).toBe(expectedTotal)
  expect(cart.discountPercentage).toBe(20)
})
```

#### Auth Store - Login con Registro Pendiente
```javascript
it('should handle login with pending registration', async () => {
  const auth = useAuthStore()
  
  const mockResponse = {
    success: true,
    data: {
      tempToken: 'temp-token',
      needsRegistration: true,
      ...
    }
  }
  
  authService.googleCallback.mockResolvedValue(mockResponse)
  
  const result = await auth.login('auth-code')
  
  expect(result).toEqual({ needsRegistration: true })
  expect(localStorage.getItem('tempToken')).toBe('temp-token')
})
```

---

## 🚀 Ejecutar Pruebas

### Script Unificado

Hemos creado un script PowerShell que ejecuta todas las pruebas:

```powershell
# Desde el directorio raíz del proyecto

# Ejecutar todas las pruebas (Backend + Frontend)
.\scripts\Run-Tests.ps1

# Solo Backend
.\scripts\Run-Tests.ps1 -Component Backend

# Solo Frontend
.\scripts\Run-Tests.ps1 -Component Frontend

# Con cobertura de código
.\scripts\Run-Tests.ps1 -Coverage
```

### Comandos Individuales

**Backend:**
```powershell
cd src/backend
dotnet test PhotosMarket.API.Tests/PhotosMarket.API.Tests.csproj
```

**Frontend:**
```powershell
cd src/frontend
npm test
```

---

## 🔄 Integración con CI/CD

### Integración con Deploy-PhotosMarket.ps1

El script de despliegue ahora **ejecuta automáticamente las pruebas** antes de construir y desplegar:

```powershell
# Desplegar con pruebas (comportamiento por defecto)
.\scripts\Deploy-PhotosMarket.ps1

# Omitir pruebas (NO RECOMENDADO para producción)
.\scripts\Deploy-PhotosMarket.ps1 -SkipTests
```

**¿Qué sucede si las pruebas fallan?**
- ❌ El despliegue se detiene inmediatamente
- 🚫 No se construyen imágenes Docker
- 🔒 No se despliega a Azure
- ⚠️ Se muestra un mensaje de error claro

### Integración con GitHub Actions

Los workflows de GitHub Actions ejecutan pruebas automáticamente:

#### Workflow CI (`ci.yml`)
Se ejecuta en **Pull Requests** y **pushes a branches**:
- ✅ Ejecuta pruebas del Backend (xUnit)
- ✅ Ejecuta pruebas del Frontend (Vitest)
- ✅ Build y validación de Docker
- ❌ Bloquea merge si las pruebas fallan

```yaml
# Ejemplo de ejecución
on:
  pull_request:
    branches: [main]
  push:
    branches-ignore: [main]

jobs:
  build-backend:
    - run: dotnet test --configuration Release
  
  build-frontend:
    - run: npm test
```

#### Workflow Deploy (`deploy-to-azure.yml`)
Se ejecuta en **pushes a main** o **manualmente**:
- 🧪 **Paso 1**: Ejecuta TODAS las pruebas (Backend + Frontend)
- 🚫 Si las pruebas fallan → Deployment CANCELADO
- ✅ Si las pruebas pasan → Continúa con build y deploy
- 🐳 Build de imágenes Docker
- ☁️ Deploy a Azure Container Apps

**Flujo de Deployment:**
```
1. Git push to main
2. ► RUN TESTS JOB ◄
   │
   ├─ Backend Tests (xUnit)
   ├─ Frontend Tests (Vitest)
   │
   ├─ ✅ All tests pass → Deploy Frontend
   │
   └─ ❌ Any test fails → CANCEL DEPLOYMENT
3. Deploy Frontend Container App
4. Deploy Backend Container App
5. Update environment vars
```

**Estados de los Jobs:**
- 🟢 **Success**: Todas las pruebas pasaron, deployment completado
- 🔴 **Failure**: Pruebas fallaron, deployment cancelado
- 🟡 **Skipped**: Job omitido por condiciones

#### Ver Resultados en GitHub

1. Ve a tu repositorio en GitHub
2. Click en **Actions** tab
3. Selecciona un workflow run
4. Revisa los logs de cada job:
   - `run-tests` - Resultados de pruebas
   - `deploy-frontend` - Deployment del frontend
   - `deploy-backend` - Deployment del backend

**Ejemplo de logs de pruebas:**
```
🧪 Running Backend Unit Tests...
✅ Backend tests passed!

🧪 Running Frontend Unit Tests...
✅ Frontend tests passed!

╔══════════════════════════════════════════════════════════╗
║      ✅ All Tests Passed! Proceeding with deployment...   ║
╚══════════════════════════════════════════════════════════╝
```

### Configurar GitHub Actions

**Secretos requeridos** (GitHub Repository → Settings → Secrets):
- `AZURE_CREDENTIALS` - Credenciales de Azure
- `GOOGLE_OAUTH_CLIENT_ID` - OAuth Client ID
- `GOOGLE_OAUTH_CLIENT_SECRET` - OAuth Secret
- Otros secretos de configuración

**Nota**: Los workflows **NO permiten** deployments si las pruebas fallan. Esto garantiza que solo código validado llega a producción.

### Flujo de Despliegue

```
1. Verificar Azure CLI y Docker
2. ► EJECUTAR PRUEBAS UNITARIAS ◄
   │
   ├─ ✅ Pruebas pasan → Continuar
   │
   └─ ❌ Pruebas fallan → DETENER DESPLIEGUE
3. Build de imágenes Docker
4. Push a Azure Container Registry
5. Despliegue a Azure Container Apps
```

---

## 📝 Mejores Prácticas

### General
1. **Ejecuta las pruebas antes de cada commit**
   ```powershell
   .\scripts\Run-Tests.ps1
   ```

2. **Nunca omitas las pruebas en producción**
   - Usa `-SkipTests` solo para desarrollo local

3. **Mantén la cobertura alta**
   - Objetivo: >80% de cobertura de código
   - Ejecuta `.\scripts\Run-Tests.ps1 -Coverage` periódicamente

4. **Escribe pruebas para nuevas funcionalidades**
   - Cada nuevo endpoint/servicio debe tener pruebas
   - Cada nuevo componente debe tener pruebas

### Backend
- ✅ Usa `[Theory]` para probar múltiples casos con un solo test
- ✅ Mockea dependencias externas (DB, API, Email)
- ✅ Verifica valores con `FluentAssertions` para mejor legibilidad
- ✅ Sigue el patrón **Arrange-Act-Assert**

### Frontend
- ✅ Mockea llamadas HTTP con `vi.mock()`
- ✅ Limpia `localStorage` en `beforeEach()`
- ✅ Prueba casos de éxito Y error
- ✅ Usa `setActivePinia()` en pruebas de stores

---

## 🔍 Solución de Problemas

### Backend

**Error: "Proyecto de pruebas no encontrado"**
```powershell
# Asegúrate de que el proyecto existe
cd src/backend/PhotosMarket.API.Tests
dotnet restore
```

**Error: "Dependencia no encontrada"**
```powershell
# Restaurar paquetes NuGet
cd src/backend
dotnet restore
```

### Frontend

**Error: "npm test: command not found"**
```powershell
# Instalar dependencias
cd src/frontend
npm install
```

**Error: "Vitest not found"**
```powershell
# Verificar que package.json tiene vitest
npm install --save-dev vitest @vue/test-utils happy-dom
```

---

## 📊 Reportes de Cobertura

### Backend
```powershell
cd src/backend
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults

# Ver reporte en: TestResults/[guid]/coverage.cobertura.xml
```

### Frontend
```powershell
cd src/frontend
npm run test:coverage

# Ver reporte HTML en: coverage/index.html
```

---

## 🎯 Pruebas Más Importantes

### Críticas para la Seguridad
- ✅ `AuthService.ValidateRut`: Validación correcta de RUT
- ✅ `AuthService.GenerateJwtToken`: Tokens JWT válidos
- ✅ `AuthController.GoogleCallback`: Manejo seguro de OAuth

### Críticas para el Negocio
- ✅ `OrderService.CreateOrderAsync`: Cálculo correcto de precios
- ✅ `OrderService.ConfirmPaymentAsync`: Actualización de estados
- ✅ `CartStore.totalAmount`: Cálculo de totales con descuentos

### Críticas para UX
- ✅ `AuthStore.login`: Flujo de autenticación completo
- ✅ `CartStore.addItem`: Prevención de duplicados
- ✅ `CartStore.clearCart`: Limpieza correcta del carrito

---

## 📚 Recursos Adicionales

- [xUnit Documentation](https://xunit.net/)
- [Moq Documentation](https://github.com/moq/moq4)
- [Vitest Documentation](https://vitest.dev/)
- [Vue Test Utils](https://test-utils.vuejs.org/)
- [FluentAssertions](https://fluentassertions.com/)

---

## ✅ Checklist de Calidad

Antes de cada despliegue, verifica:

- [ ] Todas las pruebas pasan localmente
- [ ] Cobertura de código >80%
- [ ] Nuevas funcionalidades tienen pruebas
- [ ] No hay `console.log` en código de producción
- [ ] No hay dependencias mock en código de producción
- [ ] README actualizado si hay cambios en la API

---

**¡Las pruebas son tu red de seguridad! Úsalas siempre. 🛡️**
