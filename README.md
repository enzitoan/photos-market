# 📸 PhotosMarket - Sistema de Venta de Fotografías

Aplicación web completa que permite a los clientes comprar fotografías de alta resolución desde Google Drive del fotógrafo. Sistema con backend en .NET 8.0 y frontend en Vue 3.

## 🎯 Características Principales

✨ **Para Clientes:**
- 🔐 Autenticación segura con Google OAuth 2.0
- 📁 Navegación por álbumes del fotógrafo
- 🖼️ Visualización de miniaturas con marca de agua
- 🛒 Carrito de compras para seleccionar múltiples fotos
- � Sistema de descuentos automáticos (20% desde 5 fotos)
- �📋 Proceso de pedido mediante transferencia electrónica
- 📥 Descarga de fotos en alta resolución tras confirmar el pago
- ⏰ Enlaces de descarga con vigencia de 72 horas
- 📱 Interfaz moderna y responsiva

👨‍💼 **Para Administradores:**
- 📊 Panel de administración completo
- 📦 Gestión de álbumes (bloquear/desbloquear)
- 💰 Gestión de pedidos y confirmación de pagos
- 🔗 Generación de enlaces de descarga
- ⚙️ Configuración del sistema
- 📈 Dashboard con estadísticas

🔧 **Stack Tecnológico:**
- **Backend**: .NET 8.0 Web API, Azure Cosmos DB, Google Drive API
- **Frontend**: Vue 3, Vite, Pinia, Tailwind CSS, Vue Router
- **Auth**: OAuth 2.0 + JWT
- **Emails**: MailKit (SMTP)
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
4. **Crea Pedido** → Confirma el carrito y crea el pedido
5. **Realiza Pago** → Transfiere el monto indicado
6. **Espera Confirmación** → El admin confirma el pago
7. **Descarga** → Recibe enlace para descargar fotos en alta resolución (válido 72 horas)

### **Administrador**

1. **Login Admin** → Accede con cuenta `ahumada.enzo@gmail.com`
2. **Autentica Google Drive** → Conecta con Google Drive API
3. **Gestiona Álbumes** → Bloquea/desbloquea álbumes según disponibilidad
4. **Revisa Pedidos** → Ve todos los pedidos en el sistema
5. **Confirma Pagos** → Valida transferencias recibidas y confirma en el sistema
6. **Genera Enlaces** → Sistema genera enlaces de descarga automáticamente
7. **Monitorea** → Revisa estadísticas en el dashboard

## 🛠️ Stack Tecnológico Detallado

### **Backend (.NET 8.0)**
- **ASP.NET Core Web API** - Framework principal
- **Azure Cosmos DB** - Base de datos NoSQL (con fallback a in-memory)
- **Google.Apis.Drive.v3** - Integración con Google Drive API
- **Microsoft.IdentityModel.Tokens** - Autenticación JWT
- **MailKit** - Envío de emails SMTP
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
- **Gmail SMTP** - Servidor de correo (opcional)

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

### **5. Email (Opcional)**
```json
{
  "Email": {
    "Enabled": false,  // Cambiar a true para habilitar
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "tu-email@gmail.com",
    "SenderPassword": "tu-app-password",
    "EnableSsl": true
  }
}
```

### **6. Aplicación**
```json
{
  "Application": {
    "BaseUrl": "http://localhost:5000",
    "FrontendUrl": "http://localhost:3001",
    "DownloadLinkExpirationHours": 72,
    "WatermarkText": "PhotosMarket © {YEAR}",
    "PhotoPricePerUnit": 1000,  // Precio en CLP
    "Currency": "CLP",
    "BulkDiscountMinPhotos": 5,  // Mínimo de fotos para descuento
    "BulkDiscountPercentage": 20  // Porcentaje de descuento
  }
}
```

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
- Sistema **deshabilitado por defecto** (`Email.Enabled: false`)
- Infraestructura completa implementada con MailKit
- Plantillas de email listas para usar

### **Emails Automáticos**

1. **Confirmación de Pedido**
   - Número de pedido
   - Lista de fotos seleccionadas
   - Precio total
   - Instrucciones de pago

2. **Enlace de Descarga**
   - URL con token único
   - Fecha de expiración
   - Instrucciones de descarga

### **Para Habilitar**
En `appsettings.json`:
```json
"Email": {
  "Enabled": true,
  "SenderEmail": "tu-email@gmail.com",
  "SenderPassword": "tu-app-password-aqui"
}
```

**Obtener App Password de Gmail:**
1. Cuenta Google → Seguridad
2. Verificación en 2 pasos (activar)
3. Contraseñas de aplicaciones
4. Generar nueva contraseña

## 📝 Notas Importantes

### **Marcas de Agua**
- Implementadas actualmente en el **cliente** mediante parámetros de URL de Google Drive
- Miniaturas: `400x400px` con parámetros `=w400-h400`
- Para producción: considerar implementación en servidor con SixLabors.ImageSharp

### **Google Drive API**
- Fotos servidas directamente desde Google Drive
- URLs temporales con parámetros:
  - `=w400-h400` → Miniatura 400x400 (para vista previa)
  - `=d` → Descarga en resolución original
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
- **Admin**: `ahumada.enzo@gmail.com`
- **Cliente**: `egan.fotografia.ph@gmail.com`

## 🔜 Roadmap / Mejoras Futuras

- [ ] **Registrar Usuarios** - Solicitar Rut y Fecha de Nacimiento en el primer login, guardar nombre y email de contacto.
- [ ] **Email de Confirmación** - Enviar email automático al crear pedido con instrucciones de pago.
- [x] **Sistema de Cupones** - Descuentos y promociones ✅ **IMPLEMENTADO**: Descuento del 20% desde 5 fotos
- [ ] **Enlace de Descarga por Email** - Enviar enlace de descarga automático al confirmar pago.
- [ ] **Pasarela de Pago** - Integración con Stripe/PayPal/Mercado Pago
- [ ] **Marca de Agua Server-Side** - Usar ImageSharp para mayor seguridad
- [ ] **Panel de Analytics** - Reportes de ventas y estadísticas
- [ ] **Notificaciones Push** - Websockets o Server-Sent Events
- [ ] **Multi-Storage** - Soporte para OneDrive, Dropbox, AWS S3
- [ ] **Testing** - Tests unitarios e integración
- [ ] **Cache Redis** - Mejorar performance
- [ ] **Compresión de Imágenes** - Optimización automática
- [ ] **Búsqueda Avanzada** - Por metadatos, geolocalización, fechas
- [ ] **PWA** - Aplicación web progresiva
- [ ] **Multi-idioma** - i18n (Español/Inglés)

## 📖 Documentación Adicional

- 📘 [Especificación Completa del Sistema](specification/README.md) - Arquitectura detallada, flujos, modelos de datos
- 🏗️ [Infraestructura Azure (Bicep)](infra/README.md) - Templates de Bicep, arquitectura cloud, parámetros
- 🚀 [Scripts de Despliegue](scripts/README.md) - Guía de scripts de PowerShell para deployment
- 📋 [Configuración de Scripts](scripts/CONFIG.md) - Variables de entorno y configuración de deployment
� Manual de Uso

### 🔐 Configuración Inicial

#### 1. Crear Proyecto en Google Cloud

1. Ir a [Google Cloud Console](https://console.cloud.google.com/)
2. Crear nuevo proyecto o seleccionar uno existente
3. Habilitar **Google Drive API**:
   - Ir a "APIs & Services" → "Library"
   - Buscar "Google Drive API"
   - Hacer clic en "Enable"

#### 2. Configurar OAuth 2.0

1. En Google Cloud Console, ir a "APIs & Services" → "Credentials"
2. Hacer clic en "Create Credentials" → "OAuth client ID"
3. Tipo de aplicación: **Web application**
4. Configurar:
   - **Name**: PhotosMarket
   - **Authorized redirect URIs**:
     - `http://localhost:3001/callback` (desarrollo)
     - `https://tu-dominio.com/callback` (producción)
5. Guardar **Client ID** y **Client Secret**

#### 3. Crear Credenciales de Service Account

1. En "Credentials", hacer clic en "Create Credentials" → "Service account"
2. Completar información básica
3. Descargar el archivo JSON de credenciales
4. Renombrar a `google-drive-credentials.json`
5. Colocar en `src/backend/google-drive-credentials.json`

#### 4. Configurar Google Drive

1. Crear carpeta raíz en Google Drive para las fotos
2. Compartir la carpeta con el email del Service Account
3. Copiar el **Folder ID** de la URL:
   - URL example: `https://drive.google.com/drive/folders/1ABC...XYZ`
   - Folder ID: `1ABC...XYZ`

#### 5. Configurar Variables de Entorno

**Para desarrollo local** - Editar `src/backend/appsettings.json`:

```json
{
  "GoogleOAuth": {
    "ClientId": "TU_CLIENT_ID.apps.googleusercontent.com",
    "ClientSecret": "TU_CLIENT_SECRET",
    "RedirectUri": "http://localhost:3001/callback"
  },
  "GoogleDrive": {
    "RootFolderId": "TU_FOLDER_ID_DE_GOOGLE_DRIVE"
  },
  "Jwt": {
    "SecretKey": "minimo-32-caracteres-super-secreto-para-seguridad"
  }
}
```

**Para Docker** - Editar `.env`:

```bash
GOOGLE_OAUTH_CLIENT_ID=TU_CLIENT_ID
GOOGLE_OAUTH_CLIENT_SECRET=TU_CLIENT_SECRET
GOOGLE_DRIVE_ROOT_FOLDER_ID=TU_FOLDER_ID
JWT_SECRET_KEY=tu-secreto-de-32-caracteres-minimo
```

### 👤 Guía de Usuario (Cliente)

#### 1. Registro e Inicio de Sesión

1. Acceder a la aplicación: http://localhost:3001
2. Hacer clic en **"Continuar con Google"**
3. Seleccionar cuenta de Google
4. Autorizar permisos de la aplicación
5. Automáticamente redirigido a la página de álbumes

#### 2. Explorar Álbumes

- Ver todos los álbumes disponibles en la página principal
- Hacer clic en un álbum para ver sus fotos
- Las miniaturas tienen marca de agua para protección

#### 3. Seleccionar Fotos

1. Navegar a un álbum
2. Hacer clic en una foto para ver en tamaño grande (modal)
3. Hacer clic en el botón **"Agregar al Carrito"** (icono de carrito)
4. El contador del carrito se actualiza automáticamente
5. Repetir para todas las fotos deseadas

#### 4. Crear Pedido

1. Hacer clic en el **icono del carrito** en la barra superior
2. Revisar fotos seleccionadas
3. Ver precio total calculado
4. Hacer clic en **"Crear Pedido"**
5. Se genera un número de pedido único
6. Recibir instrucciones de pago (transferencia electrónica)

#### 5. Realizar Pago

1. Realizar transferencia bancaria por el monto indicado
2. Usar como referencia el **número de pedido**
3. Esperar confirmación del fotógrafo

#### 6. Descargar Fotos

1. Una vez confirmado el pago por el administrador:
2. Recibir **email con enlace de descarga** (si emails habilitados)
3. O ir a **"Mis Pedidos"** en el menú
4. Hacer clic en el pedido confirmado
5. Hacer clic en **"Descargar Fotos"**
6. Descargar fotos en resolución original (sin marca de agua)
7. **¡Importante!** El enlace expira en **72 horas**

### 👨‍💼 Guía de Administrador

#### 1. Acceso al Panel de Administración

1. Ir a: http://localhost:3001/admin
2. Iniciar sesión con cuenta autorizada: `ahumada.enzo@gmail.com`
3. Continuar con Google OAuth

#### 2. Autenticación con Google Drive

**Primera vez:**
1. En el panel admin, ir a **"Configuración"**
2. Hacer clic en **"Autenticar Google Drive"**
3. Autorizar la aplicación para acceder a Google Drive
4. Confirmar acceso exitoso

#### 3. Gestión de Álbumes

**Ver álbumes:**
1. Ir a **"Gestión de Álbumes"**
2. Ver lista de álbumes sincronizados desde Google Drive

**Bloquear/Desbloquear álbum:**
1. Encontrar el álbum deseado
2. Hacer clic en el interruptor **"Bloqueado/Desbloqueado"**
3. Los álbumes bloqueados no son visibles para clientes
4. Usar para ocultar álbumes privados o en proceso

#### 4. Gestión de Pedidos

**Ver todos los pedidos:**
1. Ir a **"Gestión de Pedidos"**
2. Ver lista completa de pedidos con estados:
   - 🟡 **Pending** - Esperando pago
   - 🟢 **PaymentConfirmed** - Pago confirmado, listo para descarga
   - ✅ **Completed** - Pedido descargado y completado
   - ❌ **Cancelled** - Pedido cancelado

**Confirmar pago recibido:**
1. Cliente te notifica que realizó la transferencia
2. Verificar pago en tu cuenta bancaria
3. En la lista de pedidos, encontrar el pedido (estado "Pending")
4. Hacer clic en **"Confirmar Pago"**
5. El sistema automáticamente:
   - Cambia estado a "PaymentConfirmed"
   - Genera enlace de descarga único
   - Envía email al cliente (si habilitado)

**Marcar como completado:**
1. Después de que el cliente descargue las fotos
2. Hacer clic en **"Marcar Completado"**
3. El pedido pasa a estado "Completed"

#### 5. Dashboard y Reportes

1. Ir a **"Dashboard"**
2. Ver estadísticas:
   - Total de pedidos
   - Ingresos totales
   - Pedidos por estado
   - Fotos más vendidas
   - Tendencias de ventas

#### 6. Configuración del Sistema

1. Ir a **"Configuración"**
2. Ajustar:
   - Precio por foto
   - Moneda
   - Texto de marca de agua
   - Tiempo de expiración de enlaces
   - Configuración de Google Drive
   - Configuración de email

### 🔧 Casos de Uso Comunes

#### Agregar Nuevas Fotos

1. Subir fotos a la carpeta de Google Drive configurada
2. Organizar en subcarpetas (cada subcarpeta = un álbum)
3. La aplicación sincroniza automáticamente
4. Los nuevos álbumes aparecen inmediatamente
5. Si deseas ocultar temporalmente, bloquear desde el panel admin

#### Cliente No Recibe Email

**Si los emails están deshabilitados:**
1. Cliente debe revisar sus pedidos en "Mis Pedidos"
2. El enlace de descarga está disponible ahí

**Si los emails están habilitados pero no llegan:**
1. Verificar configuración SMTP en `appsettings.json`
2. Verificar que la contraseña de aplicación Gmail sea correcta
3. Revisar carpeta de spam del cliente
4. Como alternativa, el cliente puede usar "Mis Pedidos"

#### Enlace de Descarga Expirado

**Después de 72 horas:**
1. El enlace de descarga expira automáticamente
2. Cliente debe contactar al administrador
3. Administrador puede:
   - Opción 1: Generar nuevo enlace (implementar funcionalidad)
   - Opción 2: Marcar pedido como "Pending" y confirmar nuevamente
4. Se genera nuevo enlace válido por 72 horas más

#### Cambiar Precio de Fotos

1. Ir a panel de administración → **"Configuración"**
2. Modificar **"Precio por Foto"**
3. Guardar cambios
4. Los nuevos pedidos usarán el precio actualizado
5. Los pedidos existentes mantienen su precio original

#### Deshabilitar un Álbum Temporalmente

1. Panel admin → **"Gestión de Álbumes"**
2. Encontrar el álbum
3. Activar **"Bloqueado"**
4. Los clientes no verán este álbum
5. Para reactivar, desmarcar "Bloqueado"

### 🛡️ Seguridad y Buenas Prácticas

1. **Nunca compartir credenciales** en el código o repositorios
2. **Usar variables de entorno** para secretos en producción
3. **Habilitar HTTPS** en producción (automático con Azure Container Apps)
4. **Rotar JWT Secret** periódicamente
5. **Monitorear accesos** desde el panel de Log Analytics
6. **Backup de Cosmos DB** configurar snapshots automáticos
7. **Actualizar dependencias** regularmente con `npm audit` y `dotnet outdated`

## 🐛 Troubleshooting

### Backend no inicia
```powershell
# Verificar que el puerto 5000 está libre
Get-NetTCPConnection -LocalPort 5000 -ErrorAction SilentlyContinue

# Detener proceso en puerto 5000
Get-NetTCPConnection -LocalPort 5000 | Select -ExpandProperty OwningProcess | ForEach-Object { Stop-Process -Id $_ -Force }

# Verificar que .NET 8.0 SDK está instalado
dotnet --version

# Limpiar y reconstruir
cd src/backend
dotnet clean
dotnet build
```

### Frontend no inicia
```bash
# Limpiar node_modules y reinstalar
rm -rf node_modules package-lock.json
npm install

# Verificar versión de Node.js (requiere 18+)
node --version

# Limpiar cache de Vite
npm run dev -- --force
```

### Docker no construye las imágenes
```bash
# Verificar que Docker Desktop está corriendo
docker ps

# Limpiar cache de Docker
docker system prune -a

# Reconstruir desde cero
docker-compose down -v
docker-compose build --no-cache
docker-compose up -d
```

### Cosmos DB no conecta
- **Desarrollo**: Verificar `UseRealCosmosDb: false` en appsettings.json para usar in-memory
- **Producción**: Verificar connection string en Azure Portal
- **Emulador local**: Descargar e instalar [Azure Cosmos DB Emulator](https://docs.microsoft.com/azure/cosmos-db/local-emulator)
- **Warning**: In-memory storage pierde datos al reiniciar

### Google OAuth falla
- Verificar que `RedirectUri` en appsettings.json coincide **exactamente** con Google Cloud Console
- Verificar que los Scopes son correctos: `["openid", "email", "profile"]`
- Revisar que ClientId y ClientSecret sean válidos y no tengan espacios extra
- Verificar que la Authorized Redirect URI está configurada en Google Cloud Console
- Asegurarse de que la aplicación OAuth está en modo "Testing" o "Production"

### Google Drive API no encuentra fotos
- Verificar que `google-drive-credentials.json` existe en `src/backend/`
- Verificar que el Service Account tiene acceso a la carpeta de Drive
- Verificar que el `RootFolderId` es correcto
- Revisar permisos: la carpeta debe estar compartida con el Service Account email

### Errores de CORS
- Verificar que frontend está corriendo en puerto 3000, 3001, 3002 o 5173
- Verificar configuración de CORS en `Program.cs` del backend
- En producción, agregar el dominio real a la política de CORS

### Emails no se envían
- Verificar `Email.Enabled: true` en appsettings.json
- Verificar que `SenderPassword` es una **App Password** de Gmail, no la contraseña normal
- Verificar configuración SMTP:
  - Server: `smtp.gmail.com`
  - Port: `587`
  - EnableSSL: `true`
- Revisar logs del backend para errores específicos de SMTP

### Despliegue en Azure falla
```powershell
# Verificar login en Azure
az account show

# Re-login si es necesario
az login

# Verificar permisos de la suscripción
az role assignment list --assignee $(az account show --query user.name -o tsv)

# Ver logs de deployment
az deployment group show \
  --resource-group rg-photosmarket-dev \
  --name main

# Ver logs de Container Apps
az containerapp logs show \
  --name ca-photosmarket-backend-dev \
  --resource-group rg-photosmarket-dev \
  --follow
```
- Revisar que ClientId y ClientSecret sean válidos

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

**Desarrollado con** ❤️ **usando .NET Core y Azure**
