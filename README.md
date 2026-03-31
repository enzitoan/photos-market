# 📸 PhotosMarket - Sistema de Venta de Fotografías

Aplicación web completa que permite a los clientes comprar fotografías de alta resolución desde Google Drive del fotógrafo. Sistema con backend en .NET 8.0 y frontend en Vue 3.

## 🎯 Características Principales

✨ **Para Clientes:**
- 🔐 Autenticación segura con Google OAuth 2.0
- 📁 Navegación por álbumes del fotógrafo
- 🖼️ Visualización de miniaturas con marca de agua (overlay CSS)
- 🛒 Carrito de compras para seleccionar múltiples fotos
- 💰 Sistema de descuentos automáticos (20% desde 5 fotos)
- 📧 Emails automáticos: confirmación de pedido, pago confirmado y enlace de descarga
- 📋 Proceso de pedido mediante transferencia electrónica
- 📥 Descarga de fotos originales en alta resolución vía backend proxy autenticado
- ⏰ Enlaces de descarga con vigencia de 72 horas
- 📱 Interfaz moderna y responsiva

👨‍💼 **Para Administradores:**
- 📊 Panel de administración completo
- 📦 Gestión de álbumes (bloquear/desbloquear)
- 💰 Gestión de pedidos y confirmación de pagos
- � Sistema de emails automáticos en cada etapa del pedido
- 🔗 Generación automática de enlaces de descarga con expiración
- ⚙️ Configuración del sistema (precios, descuentos, emails)
- 📈 Dashboard con estadísticas

🔧 **Stack Tecnológico:**
- **Backend**: .NET 8.0 Web API, Azure Cosmos DB, Google Drive API
- **Frontend**: Vue 3, Vite, Pinia, Tailwind CSS, Vue Router
- **Auth**: OAuth 2.0 + JWT
- **Emails**: Resend API con MailKit fallback
- **Docs**: Swagger/OpenAPI

## 📁 Estructura del Proyecto

```
PhotosMarket/
├── specification/              # Especificación detallada del sistema
│   └── README.md
├── src/
│   ├── backend/               # API REST en .NET 8.0
│   │   ├── Configuration/     # Configuraciones (JWT, Cosmos, Google, Email)
│   │   ├── Controllers/       # 5 controladores REST
│   │   │   ├── AuthController.cs
│   │   │   ├── AlbumsController.cs
│   │   │   ├── PhotosController.cs
│   │   │   ├── OrdersController.cs
│   │   │   └── DownloadController.cs
│   │   ├── DTOs/             # Data Transfer Objects
│   │   ├── Models/           # Modelos de dominio (User, Order, Album, etc.)
│   │   ├── Repositories/     # Acceso a datos (Cosmos DB + In-Memory)
│   │   ├── Services/         # Lógica de negocio
│   │   │   ├── AuthService.cs
│   │   │   ├── GoogleDriveService.cs
│   │   │   ├── GoogleOAuthService.cs
│   │   │   ├── OrderService.cs
│   │   │   ├── EmailService.cs
│   │   │   ├── WatermarkService.cs
│   │   │   └── CosmosDbService.cs
│   │   ├── appsettings.json  # Configuración principal
│   │   └── Program.cs        # Punto de entrada
│   │
│   └── frontend/             # Aplicación Vue 3
│       ├── src/
│       │   ├── components/   # 6 componentes reutilizables
│       │   │   ├── AlbumCard.vue
│       │   │   ├── PhotoCard.vue
│       │   │   ├── PhotoModal.vue
│       │   │   ├── CartIcon.vue
│       │   │   ├── NavBar.vue
│       │   │   └── LoadingSpinner.vue
│       │   ├── views/        # 9 vistas públicas/cliente
│       │   │   ├── HomeView.vue
│       │   │   ├── auth/LoginView.vue
│       │   │   ├── auth/CallbackView.vue
│       │   │   ├── AlbumsView.vue
│       │   │   ├── AlbumPhotosView.vue
│       │   │   ├── CartView.vue
│       │   │   ├── OrdersView.vue
│       │   │   ├── OrderDetailView.vue
│       │   │   └── DownloadView.vue
│       │   ├── views/admin/  # 7 vistas de administración
│       │   │   ├── AdminLoginView.vue
│       │   │   ├── GoogleAuthView.vue
│       │   │   ├── AdminLayout.vue
│       │   │   ├── DashboardView.vue
│       │   │   ├── AlbumsManagementView.vue
│       │   │   ├── OrdersManagementView.vue
│       │   │   └── SettingsView.vue
│       │   ├── stores/       # Estado global (Pinia)
│       │   │   ├── auth.js
│       │   │   ├── cart.js
│       │   │   └── admin.js
│       │   ├── services/     # Servicios API
│       │   │   ├── httpClient.js
│       │   │   ├── authService.js
│       │   │   ├── photosService.js
│       │   │   ├── ordersService.js
│       │   │   ├── downloadService.js
│       │   │   └── adminService.js
│       │   ├── router/       # Configuración de rutas
│       │   │   └── index.js
│       │   ├── App.vue       # Componente raíz
│       │   └── main.js       # Entrada de la aplicación
│       ├── package.json
│       ├── vite.config.js
│       └── tailwind.config.js
│
├── SETUP.md                  # Guía de configuración rápida
├── RUNNING.md                # Instrucciones de ejecución
└── README.md                 # Este archivo
```

## 🚀 Inicio Rápido

### Prerrequisitos

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/) y npm
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (para deployment con contenedores)
- [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli) (para despliegue en Azure)
- Cuenta [Azure](https://portal.azure.com/) con Cosmos DB (opcional, tiene fallback a memoria)
- Proyecto [Google Cloud](https://console.cloud.google.com/) con Drive API habilitada
- Credenciales OAuth 2.0 de Google
- Cuenta Gmail con contraseña de aplicación (para emails, opcional)

### Instalación Rápida

1. **Configurar Backend**
   ```bash
   cd src/backend
   cp appsettings.template.json appsettings.json
   # Editar appsettings.json con tus credenciales
   dotnet restore
   dotnet build
   ```

2. **Configurar Frontend**
   ```bash
   cd src/frontend
   npm install
   ```

3. **Ejecutar Backend** (Terminal 1)
   ```bash
   cd src/backend
   dotnet run --urls "http://localhost:5000"
   ```

4. **Ejecutar Frontend** (Terminal 2)
   ```bash
   cd src/frontend
   npm run dev
   ```

5. **Acceder a la aplicación**
   - Frontend: http://localhost:3001
   - Backend API: http://localhost:5000
   - Swagger: http://localhost:5000/swagger

## 🐳 Despliegue con Docker

PhotosMarket incluye soporte completo para Docker y despliegue en Azure Container Apps.

### Ejecución Local con Docker

```bash
# 1. Copiar archivo de configuración
cp .env.example .env

# 2. Editar .env con tus credenciales
# Configurar: Google OAuth, JWT Secret, Google Drive Folder ID, etc.

# 3. Construir y ejecutar todos los servicios
docker-compose up -d

# 4. Ver logs en tiempo real
docker-compose logs -f

# 5. Ver logs de un servicio específico
docker-compose logs -f backend
docker-compose logs -f frontend

# 6. Detener servicios
docker-compose down

# 7. Reconstruir imágenes después de cambios en código
docker-compose up -d --build
```

**URLs de acceso con Docker:**
- Frontend: http://localhost:3001
- Backend API: http://localhost:5000
- Swagger: http://localhost:5000/swagger

### Despliegue en Azure Container Apps

El proyecto incluye infraestructura como código (Bicep) y scripts automatizados para desplegar en Azure:

#### Opción 1: Script Automatizado (Recomendado)

```powershell
# 1. Configurar variables de entorno requeridas
$env:GOOGLE_OAUTH_CLIENT_ID = "tu-client-id.apps.googleusercontent.com"
$env:GOOGLE_OAUTH_CLIENT_SECRET = "tu-client-secret"
$env:JWT_SECRET_KEY = "tu-jwt-secret-minimo-32-caracteres"

# 2. Ejecutar script de despliegue completo
cd scripts
.\Deploy-PhotosMarket.ps1 -ResourceGroupName "rg-photosmarket-dev" -Environment "dev"

# 3. Desplegar solo backend o frontend
.\Deploy-PhotosMarket.ps1 -Component Backend
.\Deploy-PhotosMarket.ps1 -Component Frontend

# 4. Verificar estado del despliegue
.\Get-DeploymentStatus.ps1 -ResourceGroupName "rg-photosmarket-dev"
```

#### Opción 2: Despliegue Manual con Bicep

```bash
# 1. Login a Azure
az login

# 2. Crear Resource Group
az group create \
  --name rg-photosmarket-dev \
  --location eastus

# 3. Desplegar infraestructura
az deployment group create \
  --resource-group rg-photosmarket-dev \
  --template-file infra/main.bicep \
  --parameters infra/main.bicepparam \
  --parameters environmentName=dev
```

**Recursos creados en Azure:**
- **Azure Container Registry (ACR)** - Almacenamiento de imágenes Docker
- **Azure Container Apps** - Hosting del backend (.NET 8) y frontend (Vue.js)
- **Azure Cosmos DB** - Base de datos NoSQL para almacenamiento persistente
- **Azure Key Vault** - Gestión segura de secretos y credenciales
- **Log Analytics Workspace** - Monitoreo centralizado y logs
- **Container Apps Environment** - Entorno compartido para las apps

**Configuración de marca de agua en Azure:**
Los parámetros de marca de agua (tamaño, transparencia, posición) son **configurables durante el despliegue** y **modificables después** desde Azure Portal sin redesplegar.

Ver [Guía de Configuración en Azure](docs/AZURE-WATERMARK-CONFIG.md) para personalizar en producción.

Para más detalles sobre la infraestructura, consulta:
- [Documentación de Infraestructura](infra/README.md)
- [Documentación de Scripts](scripts/README.md)

## 🔌 Endpoints API Principales

### 🔐 Autenticación (`/api/auth`)
- `GET /google-login` - Obtener URL de autenticación con Google
- `POST /google-callback?code={code}` - Callback OAuth 2.0
- `GET /validate` - Validar token JWT actual

### 📁 Álbumes (`/api/albums`)
- `GET /` - Listar todos los álbumes disponibles
- `GET /{albumId}` - Obtener detalles de un álbum específico
- `POST /{albumId}/toggle-block` - Bloquear/desbloquear álbum [Admin]

### 🖼️ Fotos (`/api/photos`)
- `GET /albums` - Listar álbumes con fotos
- `GET /albums/{albumId}/photos` - Obtener fotos de un álbum
- `GET /{mediaItemId}` - Obtener detalle de una foto específica

### 📦 Pedidos (`/api/orders`)
- `POST /` - Crear nuevo pedido
- `GET /` - Listar pedidos del usuario autenticado
- `GET /all` - Listar todos los pedidos [Admin]
- `GET /{orderId}` - Obtener detalle de un pedido
- `POST /{orderId}/confirm-payment` - Confirmar pago recibido [Admin]
- `POST /{orderId}/complete` - Marcar como completado [Admin]

### 📥 Descargas (`/api/download`)
- `GET /{token}` - Verificar enlace de descarga
- `GET /{token}/files` - Obtener URLs de descarga de fotos
- `POST /{orderId}/generate` - Generar enlace de descarga [Admin]

## 🔄 Flujo de Trabajo

### **Cliente**

1. **Login** → Hace clic en "Continuar con Google" → Autoriza la app
2. **Explora Álbumes** → Navega por álbumes disponibles
3. **Selecciona Fotos** → Agrega fotos al carrito (miniaturas con marca de agua)
4. **Crea Pedido** → Confirma el carrito → Recibe email de confirmación
5. **Realiza Pago** → Transfiere el monto indicado
6. **Espera Confirmación** → El admin confirma el pago → Recibe email de pago confirmado
7. **Descarga** → Recibe email con enlace de descarga → Descarga fotos originales en alta resolución desde backend proxy autenticado (válido 72 horas)

### **Administrador**

1. **Login Admin** → Accede con cuenta `ahumada.enzo@gmail.com`
2. **Autentica Google Drive** → Conecta con Google Drive API
3. **Gestiona Álbumes** → Bloquea/desbloquea álbumes según disponibilidad
4. **Revisa Pedidos** → Ve todos los pedidos en el sistema
5. **Confirma Pagos** → Valida transferencias recibidas y confirma en el sistema → Sistema envía email de confirmación al cliente
6. **Completa Orden** → Marca orden como completada → Sistema genera enlace y envía email con link de descarga
7. **Monitorea** → Revisa estadísticas en el dashboard

## 🛠️ Stack Tecnológico Detallado

### **Backend (.NET 8.0)**
- **ASP.NET Core Web API** - Framework principal
- **Azure Cosmos DB** - Base de datos NoSQL (con fallback a in-memory)
- **Google.Apis.Drive.v3** - Integración con Google Drive API
- **Microsoft.IdentityModel.Tokens** - Autenticación JWT
- **Resend SDK** - Servicio de emails transaccionales con MailKit como fallback
- **Swashbuckle (Swagger)** - Documentación OpenAPI

### **Frontend (Vue 3)**
- **Vue 3.4+** - Framework JavaScript progresivo
- **Vite 5** - Build tool ultrarrápido
- **Vue Router 4** - Enrutamiento SPA
- **Pinia** - State management oficial de Vue
- **Axios** - Cliente HTTP con interceptores
- **Tailwind CSS 3** - Framework de utilidades CSS
- **Vue Toastification** - Sistema de notificaciones toast
- **ESLint** - Linter para calidad de código

### **Infraestructura**
- **Google Cloud Platform** - OAuth 2.0, Drive API
- **Azure Cosmos DB** - Base de datos NoSQL
- **Resend** - Servicio de emails transaccionales

## 📋 Configuración Principal

El archivo `src/backend/appsettings.json` contiene:

### **1. Cosmos DB**
```json
{
  "UseRealCosmosDb": false,  // true para producción
  "ConnectionStrings": {
    "CosmosDb": "AccountEndpoint=https://..."
  },
  "CosmosDb": {
    "DatabaseName": "PhotosMarketDb",
    "ContainerNames": {
      "Orders": "Orders",
      "Users": "Users",
      "DownloadLinks": "DownloadLinks",
      "PhotographerSettings": "PhotographerSettings"
    }
  }
}
```
**Nota**: Si Cosmos DB no está disponible, automáticamente usa almacenamiento en memoria.

### **2. Google Drive**
```json
{
  "GoogleDrive": {
    "CredentialsFilePath": "google-drive-credentials.json",
    "RootFolderId": "ID_DE_TU_CARPETA_RAIZ",
    "ApplicationName": "PhotosMarket"
  }
}
```

### **3. Google OAuth** 
```json
{
  "GoogleOAuth": {
    "ClientId": "tu-client-id.apps.googleusercontent.com",
    "ClientSecret": "tu-client-secret",
    "RedirectUri": "http://localhost:3001/callback",
    "Scopes": ["openid", "email", "profile"]
  }
}
```

### **4. JWT**
```json
{
  "Jwt": {
    "SecretKey": "minimo-32-caracteres-para-seguridad",
    "Issuer": "PhotosMarketAPI",
    "Audience": "PhotosMarketClient",
    "ExpirationInMinutes": 60
  }
}
```

### **5. Email**
```json
{
  "Email": {
    "Enabled": true,  // Habilitar/deshabilitar sistema de emails
    "Provider": "Resend", // "Resend" o "Smtp"
    "SenderEmail": "onboarding@resend.dev",
    "SenderName": "PhotosMarket",
    
    // Configuración Resend (recomendado)
    "ResendApiKey": "re_tu_api_key_aqui",
    
    // Configuración SMTP alternativa (Gmail, etc.)
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderPassword": "tu-app-password",
    "EnableSsl": true
  }
}
```

**Nota**: El sistema usa Resend por defecto para emails transaccionales profesionales. SMTP está disponible como alternativa.

### **6. Aplicación**
```json
{
  "Application": {
    "BaseUrl": "http://localhost:5000",
    "FrontendUrl": "http://localhost:3001",
    "DownloadLinkExpirationHours": 72,
    "WatermarkText": "PhotosMarket © {YEAR}",
    "DefaultWatermarkText": "@egan.fotografia",  // Texto de marca de agua en descargas
    "PhotoPricePerUnit": 1000,  // Precio en CLP
    "Currency": "CLP",
    "BulkDiscountMinPhotos": 5,  // Mínimo de fotos para descuento
    "BulkDiscountPercentage": 20,  // Porcentaje de descuento
    
    // Configuración de Marca de Agua (Personalizables)
    "WatermarkFontSizeDivisor": 30,      // Menor = marca más grande (20-50)
    "WatermarkTextOpacity": 0.8,         // Transparencia texto (0.0-1.0)
    "WatermarkShadowOpacity": 0.7,       // Transparencia sombra (0.0-1.0)
    "WatermarkVerticalPosition": 0.9     // Posición vertical (0.0=arriba, 1.0=abajo)
  }
}
```

**Configuración de Marca de Agua:**
- **DefaultWatermarkText**: Texto que aparece en las fotos descargadas (ej: `@egan.fotografia`)
- **WatermarkFontSizeDivisor**: Tamaño de fuente (menor número = marca más grande)
  - `20` = muy grande, `30` = mediano (default), `40` = pequeño
- **WatermarkTextOpacity**: Opacidad del texto blanco (0.0-1.0)
  - `0.6` = sutil, `0.8` = balanceado (default), `1.0` = opaco
- **WatermarkShadowOpacity**: Opacidad de la sombra negra (0.0-1.0)
  - `0.5` = suave, `0.7` = balanceado (default), `0.9` = pronunciado
- **WatermarkVerticalPosition**: Posición vertical (0.0-1.0)
  - `0.5` = centro, `0.9` = inferior (default), `0.95` = muy abajo

Para guía completa de personalización, ver [docs/WATERMARK-CUSTOMIZATION.md](docs/WATERMARK-CUSTOMIZATION.md)

**Sistema de Descuentos:**
- **BulkDiscountMinPhotos**: Cantidad mínima de fotos para aplicar descuento (default: 5)
- **BulkDiscountPercentage**: Porcentaje de descuento a aplicar (default: 20%)
- Ejemplo: Con 5 fotos a $1,000 c/u → Subtotal $5,000 → Descuento 20% = -$1,000 → **Total $4,000**

Ver `appsettings.template.json` como referencia completa.

## 🔐 Seguridad

- ✅ **OAuth 2.0** - Autenticación con Google (standard industry)
- ✅ **JWT Tokens** - Sesiones seguras con expiración de 60 minutos
- ✅ **HTTPS** - Recomendado en producción
- ✅ **CORS** - Configurado para frontend específico (puertos 3000-3002, 5173)
- ✅ **Validación de Tokens** - Middleware en cada request protegido
- ✅ **Partition Keys** - Aislamiento de datos en Cosmos DB
- ✅ **Enlaces Temporales** - Descarga con expiración de 72 horas
- ✅ **Roles** - Separación User/Admin
- ✅ **Secrets** - Nunca en código, solo en configuración

## 📧 Sistema de Emails

### **Estado Actual**
- Sistema **habilitado** con Resend API
- Infraestructura completa implementada con Resend SDK y MailKit como fallback
- 3 plantillas de email profesionales listas para usar
- Emails automáticos en cada etapa del pedido

### **Emails Automáticos**

El sistema envía 3 correos automáticos durante el ciclo de vida del pedido:

1. **Confirmación de Pedido** (al crear orden)
   - Número de pedido
   - Lista de fotos seleccionadas
   - Precio total y descuentos aplicados
   - Instrucciones de pago
   - Estado: En Espera de Pago

2. **Pago Confirmado** (al validar la transferencia)
   - Confirmación de pago recibido
   - Resumen del pedido
   - Fecha y referencia de pago
   - Aviso de que recibirá el enlace de descarga próximamente

3. **Orden Completa con Enlace de Descarga** (al completar orden)
   - URL con token único de descarga
   - Fecha de expiración (72 horas)
   - Instrucciones de descarga
   - Advertencia de vigencia del enlace

### **Configuración**

El sistema viene configurado con Resend API. Para usar tu propia cuenta:

**Opción 1: Resend (Recomendado)**
1. Crear cuenta en [Resend.com](https://resend.com)
2. Obtener API Key desde el dashboard
3. Configurar en `appsettings.json`:
```json
"Email": {
  "Enabled": true,
  "ResendApiKey": "re_tu_api_key",
  "SenderEmail": "noreply@tu-dominio.com",
  "SenderName": "PhotosMarket"
}
```

**Opción 2: SMTP / Gmail**
1. Obtener App Password de Gmail:
   - Cuenta Google → Seguridad
   - Verificación en 2 pasos (activar)
   - Contraseñas de aplicaciones → Generar
2. Configurar en `appsettings.json`:
```json
"Email": {
  "Enabled": true,
  "Provider": "Smtp",
  "SmtpServer": "smtp.gmail.com",
  "SmtpPort": 587,
  "SenderEmail": "tu-email@gmail.com",
  "SenderPassword": "tu-app-password",
  "EnableSsl": true
}
```

## 📝 Notas Importantes

### **Marcas de Agua**
- **Vista previa (PhotoCard)**: Overlay CSS con marca de agua sobre miniaturas
- **Descarga post-compra**: Marca de agua permanente server-side con ImageSharp (`@egan.fotografia`)
  - Aplicada automáticamente al descargar fotos después de finalizar el pedido
  - Posición: Centro/inferior de la imagen (configurable)
  - Renderizado server-side con SixLabors.ImageSharp para máxima seguridad
  - Incluye sombra para mejor visibilidad en cualquier fondo
  - **Tamaño y transparencia 100% configurables** desde `appsettings.json` (sin tocar código)
  - Ver [Guía de Personalización](docs/WATERMARK-CUSTOMIZATION.md) para ajustar tamaño, opacidad y posición
- Miniaturas: `400x400px` usando URLs de Google Drive con parámetro `=w400`

### **Sistema de Descarga**
- **Backend como proxy autenticado**: Las descargas se realizan a través del backend
- El backend usa Google Drive API oficial para descargar archivos
- **Marca de agua automática**: Todas las fotos descargadas incluyen marca de agua `@egan.fotografia` en el centro/inferior
- **Procesamiento server-side**: Usa SixLabors.ImageSharp para renderizar marca de agua permanente
- **Metadatos preservados**: EXIF, fecha, cámara, etc. se mantienen intactos
- **Seguridad**: El frontend nunca accede directamente a Google Drive
- **Validación**: Token de descarga validado en cada petición
- **Protección**: Marca de agua no removible (renderizada en la imagen, no overlay CSS)

### **Google Drive API**
- Fotos almacenadas en Google Drive del fotógrafo
- URLs de Google Drive para miniaturas:
  - `=w400` → Miniatura 400x400 (para vista previa)
- Descarga mediante API oficial de Google Drive (resolución original)
- No hay almacenamiento local de imágenes

### **Cosmos DB**
- **Producción**: Usar Azure Cosmos DB real
- **Desarrollo**: Emulador local o fallback a memoria
- **Fallback automático**: Si Cosmos no conecta → In-Memory storage
- **Advertencia In-Memory**: Los datos se pierden al reiniciar

### **CORS**
Puertos permitidos por defecto:
- `http://localhost:3000` - React default
- `http://localhost:3001` - Vue/Vite custom
- `http://localhost:3002` - Backup
- `http://localhost:5173` - Vite default

### **Precios**
- Moneda: **Pesos Chilenos (CLP)**
- Precio por foto: **$1.000 CLP**
- Configurable en `Application.PhotoPricePerUnit`

### **Usuarios de Prueba**
- **Admin**: `egan.fotografia.ph@gmail.com`
- **Cliente**: `ahumada.enzo@gmail.com`

## 🔜 Roadmap / Mejoras Futuras

- [ ] **Pasarela de Pago** - Integración con Stripe/PayPal/Mercado Pago.
- [ ] **Compresión de Imágenes** - Optimización automática.
- [ ] **Multi-Storage** - Soporte para OneDrive, Dropbox, AWS S3.
- [ ] **Cache Redis** - Mejorar performance.
- [ ] **Azure App Configuration** - Gestión centralizada de configuración con feature flags, versionado, y configuración dinámica sin redespliegues.
- [ ] **Búsqueda Avanzada** - Por metadatos, geolocalización, fechas.
- [ ] **Multi-idioma** - i18n (Español/Inglés)
- [ ] **Convertir en SAAS** - Permitir múltiples fotógrafos con subdominios personalizados.

## 📖 Documentación Adicional

- 📘 [Especificación Completa del Sistema](specification/README.md) - Arquitectura detallada, flujos, modelos de datos
- 🏗️ [Infraestructura Azure (Bicep)](infra/README.md) - Templates de Bicep, arquitectura cloud, parámetros
- 🚀 [Scripts de Despliegue](scripts/README.md) - Guía de scripts de PowerShell para deployment
- 📋 [Configuración de Scripts](scripts/CONFIG.md) - Variables de entorno y configuración de deployment
- 📚 [Manual de Usuario Completo](docs/USER_MANUAL.md) - Guía detallada para configuración, uso y administración del sistema

## 🤝 Contribución

Este es un proyecto privado. Para cambios:
1. Crear una rama con nombre descriptivo
2. Implementar cambios
3. Testear localmente
4. Crear pull request con descripción detallada

## 📄 Licencia

Este proyecto es de **uso privado y propietario**.

---

**Desarrollado con ❤️ para fotógrafos profesionales**

**Desarrollado con ❤️ usando .NET Core y Azure**

**Desarrollado con ❤️ por Enzo Ahumada**

**Co-creado con IA y GitHub Copilot para máxima eficiencia y calidad**
