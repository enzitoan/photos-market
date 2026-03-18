# 📸 PhotosMarket - Sistema de Venta de Fotografías

Aplicación web completa que permite a los clientes comprar fotografías de alta resolución desde Google Drive del fotógrafo. Sistema con backend en .NET 8.0 y frontend en Vue 3.

## 🎯 Características Principales

✨ **Para Clientes:**
- 🔐 Autenticación segura con Google OAuth 2.0
- 📁 Navegación por álbumes del fotógrafo
- 🖼️ Visualización de miniaturas con marca de agua
- 🛒 Carrito de compras para seleccionar múltiples fotos
- 📋 Proceso de pedido mediante transferencia electrónica
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

Para instrucciones detalladas, consulta [SETUP.md](SETUP.md) y [RUNNING.md](RUNNING.md).

## 🐳 Despliegue con Docker

PhotosMarket incluye soporte completo para Docker y despliegue en Azure.

### Ejecución Local con Docker

```bash
# Copiar archivo de configuración
cp .env.example .env
# Editar .env con tus credenciales

# Construir y ejecutar
docker-compose up -d

# Ver logs
docker-compose logs -f

# Detener
docker-compose down
```

### Despliegue en Azure

El proyecto incluye scripts de Bicep para desplegar en Azure Container Apps:

```powershell
# Configurar variables de entorno
$env:GOOGLE_OAUTH_CLIENT_ID = "tu-client-id"
$env:GOOGLE_OAUTH_CLIENT_SECRET = "tu-client-secret"
$env:JWT_SECRET_KEY = "tu-jwt-secret-key"

# Desplegar a Azure
cd scripts
.\deploy-azure.ps1 -ResourceGroupName "rg-photosmarket-dev" -Environment "dev"
```

**Recursos creados en Azure:**
- Azure Container Registry (ACR) para imágenes Docker
- Azure Container Apps para backend y frontend
- Azure Cosmos DB para base de datos
- Azure Key Vault para gestión de secretos
- Log Analytics para monitoreo

Para más detalles, consulta [DOCKER-DEPLOYMENT.md](DOCKER-DEPLOYMENT.md).

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
    "Currency": "CLP"
  }
}
```

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

- [ ] **Pasarela de Pago** - Integración con Stripe/PayPal/Mercado Pago
- [ ] **Marca de Agua Server-Side** - Usar ImageSharp para mayor seguridad
- [ ] **Panel de Analytics** - Reportes de ventas y estadísticas
- [ ] **Notificaciones Push** - Websockets o Server-Sent Events
- [ ] **Sistema de Cupones** - Descuentos y promociones
- [ ] **Multi-Storage** - Soporte para OneDrive, Dropbox, AWS S3
- [ ] **Testing** - Tests unitarios e integración
- [ ] **Cache Redis** - Mejorar performance
- [ ] **Compresión de Imágenes** - Optimización automática
- [ ] **Búsqueda Avanzada** - Por metadatos, geolocalización, fechas
- [ ] **PWA** - Aplicación web progresiva
- [ ] **Multi-idioma** - i18n (Español/Inglés)

## 📖 Documentación Adicional

- 📘 [Especificación Completa del Sistema](specification/README.md) - Arquitectura detallada, flujos, modelos
- 🔧 [Guía de Configuración](SETUP.md) - Setup paso a paso de Google Cloud, Azure, etc.
- ▶️ [Instrucciones de Ejecución](RUNNING.md) - Comandos para iniciar el sistema

## 🐛 Troubleshooting

### Backend no inicia
```bash
# Verificar que el puerto 5000 está libre
Get-NetTCPConnection -LocalPort 5000 -ErrorAction SilentlyContinue

# Detener proceso en puerto 5000
Get-NetTCPConnection -LocalPort 5000 | Select -ExpandProperty OwningProcess | ForEach-Object { Stop-Process -Id $_ -Force }
```

### Frontend no inicia
```bash
# Limpiar node_modules y reinstalar
rm -rf node_modules package-lock.json
npm install
```

### Cosmos DB no conecta
- Verificar que el emulador está corriendo (Windows)
- O configurar `UseRealCosmosDb: false` para usar in-memory

### Google OAuth falla
- Verificar que `RedirectUri` coincide con Google Cloud Console
- Verificar que los Scopes están correctos
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
