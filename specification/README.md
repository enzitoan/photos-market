# 📸 PhotosMarket - Especificación del Sistema

## 🎯 Descripción General

PhotosMarket es una aplicación web completa que permite a los clientes comprar fotografías de alta resolución almacenadas en Google Drive del fotógrafo. La aplicación actúa como una galería intermedia entre el fotógrafo y sus clientes, proporcionando una interfaz moderna y segura para la visualización, selección y compra de fotografías.

## 🚀 Actualizaciones Recientes

### **Nuevas Características**
- ✨ **Carrito flotante mejorado** - Botón flotante en dispositivos móviles con mejor experiencia de usuario
- 🎨 **Protección mejorada de imágenes** - Deshabilitación de clic derecho, arrastrar y selección de texto en componentes de galería
- 📥 **Gestión de enlaces de descarga** - Funcionalidad completa para generar y gestionar enlaces de descarga de órdenes completadas
- 🖼️ **Marca de agua avanzada** - Soporte para orientación EXIF, mejor visualización y personalización de marca de agua
- 📱 **Notificaciones mejoradas** - Mejor presentación de notificaciones en vista móvil
- 💰 **Actualización de terminología** - Revisión de términos relacionados con precios en toda la aplicación
- 📧 **Mejoras en emails** - Plantillas actualizadas para confirmación de pago y notificaciones de estado

### **Mejoras Técnicas**
- Carga asincrónica de credenciales de Google Drive
- URLs de descarga con fallbacks y manejo mejorado de base URL del backend
- Mejor gestión de flujos en el servicio de marca de agua
- Integración con SkiaSharp para mejor procesamiento de imágenes

## 🏗️ Arquitectura del Sistema

### **Backend** (API REST)
- **Framework**: .NET 8.0 Web API
- **Puerto**: `http://localhost:5000`
- **Base de Datos**: Azure Cosmos DB (con fallback a almacenamiento en memoria)
- **Autenticación**: OAuth 2.0 (Google) + JWT
- **Integración**: Google Drive API
- **Documentación**: Swagger UI disponible en `/swagger`

### **Frontend** (Aplicación Web)
- **Framework**: Vue 3 con Composition API
- **Build Tool**: Vite
- **Puerto**: `http://localhost:3001`
- **Estado Global**: Pinia
- **Enrutamiento**: Vue Router
- **Estilos**: Tailwind CSS
- **Notificaciones**: Vue Toastification
- **HTTP Client**: Axios

## 🔐 Sistema de Autenticación

### **Para Clientes**
- Autenticación mediante **Google OAuth 2.0**
- Los usuarios inician sesión con su cuenta de Google
- Sistema de tokens JWT para mantener la sesión
- Tokens con expiración de 60 minutos (configurable)

### **Para Administradores**
- Cuenta administradora: `ahumada.enzo@gmail.com`
- Acceso al panel de administración mediante autenticación Google
- Permisos especiales para gestionar pedidos y configuración

### **Cuenta de Prueba (Cliente)**
- Email: `egan.fotografia.ph@gmail.com`
- Rol: Cliente para pruebas de funcionalidad

## 📋 Funcionalidades del Sistema

### 🛍️ **Para Clientes**

#### 1. Autenticación y Acceso
- Login mediante Google OAuth 2.0
- Validación automática de sesión
- Redirección segura después de autenticación

#### 2. Navegación de Álbumes
- Vista de todos los álbumes disponibles
- Miniaturas de fotos con marca de agua
- Información de cada álbum (título, cantidad de fotos)

#### 3. Visualización de Fotos
- Galería de fotos por álbum
- Vista modal para ver fotos en tamaño grande
- Miniaturas con marca de agua (400x400px)
- Carga optimizada de imágenes

#### 4. Carrito de Compras
- Selección múltiple de fotos
- Agregar/eliminar fotos del carrito
- Vista previa del carrito con contador
- Precio total calculado automáticamente

#### 5. Gestión de Pedidos
- Crear pedido desde el carrito
- Ver historial de pedidos
- Ver detalle de cada pedido
- Estados de pedido: Pendiente, Pagado, Completado

#### 6. Descarga de Fotos
- Enlaces de descarga con token único
- Validez de 72 horas (configurable)
- Descarga de fotos en alta resolución
- Verificación de expiración del enlace

### 👨‍💼 **Para Administradores**

#### 1. Panel de Administración
- Dashboard con estadísticas generales
- Vista unificada de todos los pedidos
- Gestión centralizada del sistema

#### 2. Gestión de Álbumes
- Listar todos los álbumes de Google Drive
- Bloquear/desbloquear álbumes
- Visualizar álbumes disponibles para clientes
- Sincronización con Google Drive

#### 3. Gestión de Pedidos
- Ver todos los pedidos del sistema
- Filtrar pedidos por estado
- Confirmar pagos recibidos
- Marcar pedidos como completados
- Generar enlaces de descarga

#### 4. Configuración del Sistema
- Configurar autenticación con Google Drive
- Gestionar ajustes de la aplicación
- Configurar precios y moneda
- Ajustar marca de agua

## 🔄 Flujo de Trabajo Completo

### **Flujo del Cliente**

1. **Autenticación**
   - Accede a `/login`
   - Hace clic en "Continuar con Google"
   - Autoriza permisos de la aplicación
   - Redirección automática a álbumes

2. **Navegación y Selección**
   - Explora álbumes disponibles en `/albums`
   - Selecciona un álbum específico `/albums/:id`
   - Ve fotos con marca de agua
   - Agrega fotos al carrito

3. **Proceso de Compra**
   - Revisa el carrito en `/cart`
   - Confirma la selección
   - Crea el pedido
   - Recibe número de pedido

4. **Seguimiento**
   - Accede a sus pedidos en `/orders`
   - Ve el detalle del pedido `/orders/:id`
   - Espera confirmación de pago

5. **Descarga**
   - Recibe enlace de descarga por email (cuando esté habilitado)
   - Accede a `/download/:token`
   - Descarga fotos en alta resolución

### **Flujo del Administrador**

1. **Acceso al Panel**
   - Login en `/admin/login`
   - Autenticación con Google
   - Acceso a `/admin/dashboard`

2. **Autenticación con Google Drive**
   - Configura conexión en `/admin/google-auth`
   - Autoriza acceso a Google Drive
   - Verifica sincronización de álbumes

3. **Gestión de Pedidos**
   - Revisa pedidos en `/admin/orders`
   - Verifica transferencias bancarias
   - Confirma pagos en el sistema
   - Genera enlaces de descarga

4. **Configuración**
   - Ajusta precios en `/admin/settings`
   - Configura marca de agua
   - Gestiona álbumes disponibles

## 🗂️ Estructura de Datos

### **Modelos del Backend**

#### User (Usuario)
```csharp
- Id: string
- Email: string
- Name: string
- GoogleId: string
- Role: string (User/Admin)
- CreatedAt: DateTime
```

#### Album (Álbum)
```csharp
- Id: string
- Title: string
- CoverPhotoUrl: string
- PhotoCount: int
- IsBlocked: bool
- CreatedAt: DateTime
```

#### Order (Pedido)
```csharp
- Id: string
- UserId: string
- UserEmail: string
- PhotoIds: List<string>
- TotalAmount: decimal
- Currency: string
- Status: string (Pending/Paid/Completed)
- CreatedAt: DateTime
- PaidAt: DateTime?
- CompletedAt: DateTime?
```

#### DownloadLink (Enlace de Descarga)
```csharp
- Id: string
- OrderId: string
- Token: string
- ExpiresAt: DateTime
- CreatedAt: DateTime
- IsUsed: bool
```

## 🔌 API Endpoints

### **Autenticación** (`/api/auth`)
- `GET /google-login` - Obtener URL de autenticación con Google
- `POST /google-callback` - Callback OAuth 2.0 para clientes
- `POST /photographer-google-callback` - Callback OAuth 2.0 para fotógrafos
- `POST /admin-login` - Login de administrador
- `GET /validate` - Validar token JWT actual
- `POST /complete-registration` - Completar registro con datos adicionales

### **Fotos** (`/api/photos`)
- `GET /config` - Obtener configuración pública del fotógrafo (sin autenticación)
- `GET /albums` - Listar álbumes disponibles
- `GET /albums/{albumId}` - Obtener detalle de un álbum específico
- `GET /albums/{albumId}/photos` - Obtener fotos de un álbum
- `GET /proxy/{fileId}` - Proxy de imágenes desde Google Drive (sin autenticación)

### **Álbumes Administración** (`/api/admin/albums`)
- `GET /` - Listar todos los álbumes y su configuración (Admin)
- `POST /{googleAlbumId}/block` - Bloquear un álbum (Admin)
- `GET /photographer-settings` - Obtener configuración del fotógrafo

### **Pedidos** (`/api/orders`)
- `POST /` - Crear nuevo pedido
- `GET /` - Listar pedidos del usuario autenticado
- `GET /{orderId}` - Obtener detalle de un pedido específico
- `POST /{orderId}/confirm-payment` - Confirmar pago recibido (Admin)
- `POST /{orderId}/complete` - Marcar pedido como completado (Admin)
- `POST /{orderId}/cancel` - Cancelar un pedido
- `GET /{orderId}/download-link` - Obtener o crear link de descarga de una orden
- `POST /{orderId}/regenerate-download-link` - Regenerar link de descarga
- `GET /admin/all` - Listar todos los pedidos del sistema (Admin)

### **Descargas** (`/api/download`)
- `GET /{token}` - Verificar validez del link de descarga
- `GET /{token}/files` - Obtener URLs de descarga de todas las fotos
- `GET /{token}/photo/{photoId}` - Descargar foto individual con marca de agua

## 🎨 Vistas del Frontend

### **Públicas**
- `/` - Página de inicio (HomeView)
- `/login` - Inicio de sesión con Google (LoginView)
- `/callback` - Callback OAuth 2.0 (CallbackView)
- `/download/:token` - Descarga de fotos (DownloadView)

### **Clientes Autenticados**
- `/albums` - Listado de álbumes (AlbumsView)
- `/albums/:id` - Fotos de un álbum (AlbumPhotosView)
- `/cart` - Carrito de compras (CartView)
- `/orders` - Historial de pedidos (OrdersView)
- `/orders/:id` - Detalle de pedido (OrderDetailView)

### **Panel de Administración**
- `/admin/login` - Login de administrador (AdminLoginView)
- `/admin/google-auth` - Autenticación Google Drive (GoogleAuthView)
- `/admin/dashboard` - Dashboard principal (DashboardView)
- `/admin/albums` - Gestión de álbumes (AlbumsManagementView)
- `/admin/orders` - Gestión de pedidos (OrdersManagementView)
- `/admin/settings` - Configuración del sistema (SettingsView)

## 🔧 Configuración del Sistema

### **Variables de Configuración** (`appsettings.json`)

#### Cosmos DB
```json
{
  "UseRealCosmosDb": false,
  "ConnectionStrings": {
    "CosmosDb": "AccountEndpoint=https://localhost:8081/..."
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

#### Google Drive
```json
{
  "GoogleDrive": {
    "CredentialsFilePath": "google-drive-credentials.json",
    "RootFolderId": "1JoezTDvrHG76ICArjBhFBdZEOW8tWn05",
    "ApplicationName": "PhotosMarket"
  }
}
```

#### Google OAuth
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

#### JWT
```json
{
  "Jwt": {
    "SecretKey": "clave-secreta-minimo-32-caracteres",
    "Issuer": "PhotosMarketAPI",
    "Audience": "PhotosMarketClient",
    "ExpirationInMinutes": 60
  }
}
```

#### Email (SMTP)
```json
{
  "Email": {
    "Enabled": false,
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "tu-email@gmail.com",
    "SenderPassword": "tu-app-password",
    "EnableSsl": true
  }
}
```

#### Aplicación
```json
{
  "Application": {
    "BaseUrl": "http://localhost:5000",
    "FrontendUrl": "http://localhost:3001",
    "DownloadLinkExpirationHours": 72,
    "WatermarkText": "PhotosMarket © {YEAR}",
    "PhotoPrice": 1000,
    "Currency": "CLP"
  }
}
```

## 🛠️ Stack Tecnológico Completo

### **Backend**
- **.NET 8.0** - Framework principal
- **ASP.NET Core** - Web API
- **Azure Cosmos DB** - Base de datos NoSQL
- **Google.Apis.Drive.v3** - Integración Google Drive
- **JWT Bearer Authentication** - Autenticación
- **MailKit** - Envío de emails
- **Swagger/OpenAPI** - Documentación de API

### **Frontend**
- **Vue 3** - Framework JavaScript reactivo
- **Vite** - Build tool y dev server
- **Vue Router** - Enrutamiento SPA
- **Pinia** - Gestión de estado
- **Axios** - Cliente HTTP
- **Tailwind CSS** - Framework de estilos
- **Vue Toastification** - Notificaciones toast
- **ESLint** - Análisis de código

## 💾 Persistencia de Datos

### **Modo Producción**
- Utiliza **Azure Cosmos DB** para persistencia
- Configuración en `appsettings.json`
- Conexión verificada al inicio

### **Modo Desarrollo/Fallback**
- Sistema de **almacenamiento en memoria**
- Activado automáticamente si Cosmos DB no está disponible
- Datos se pierden al reiniciar la aplicación
- Útil para desarrollo y pruebas locales

## 🔒 Seguridad Implementada

- ✅ **OAuth 2.0** para autenticación de usuarios
- ✅ **Tokens JWT** para sesiones
- ✅ **CORS** configurado para frontend específico
- ✅ **HTTPS** recomendado en producción
- ✅ **Validación de tokens** en cada request protegido
- ✅ **Partition keys** en Cosmos DB para aislamiento
- ✅ **Enlaces de descarga con expiración** (72 horas)
- ✅ **Separación de roles** (User/Admin)

## 📧 Sistema de Notificaciones

### **Estado Actual**
- Sistema de email **deshabilitado por defecto** (`Enabled: false`)
- Infraestructura preparada con MailKit
- Plantillas de email listas

### **Emails Programados**

1. **Email de Confirmación de Pedido**
   - Enviado al crear un pedido
   - Incluye: número de pedido, lista de fotos, precio total
   - Instrucciones de pago por transferencia

2. **Email de Enlace de Descarga**
   - Enviado al confirmar el pago
   - Incluye: enlace de descarga, fecha de expiración
   - Instrucciones para descargar fotos

### **Activación**
Para habilitar emails, actualizar en `appsettings.json`:
```json
"Email": {
  "Enabled": true,
  "SmtpServer": "smtp.gmail.com",
  "SenderEmail": "tu-email@gmail.com",
  "SenderPassword": "tu-app-password"
}
```

## 🚀 Instrucciones de Ejecución

### **Backend**
```bash
cd src/backend
dotnet restore
dotnet build
dotnet run --urls "http://localhost:5000"
```

### **Frontend**
```bash
cd src/frontend
npm install
npm run dev
```

### **Acceso**
- **Frontend**: http://localhost:3001
- **Backend API**: http://localhost:5000
- **Swagger**: http://localhost:5000/swagger

## 📝 Notas Importantes

1. **Marcas de Agua**: Actualmente aplicadas en el cliente mediante parámetros de URL de Google Drive, mostrando miniaturas de 400x400px.

2. **Google Drive API**: Las fotos se sirven directamente desde Google Drive usando URLs con parámetros:
   - `=w400-h400` para miniaturas
   - `=d` para descarga en alta resolución

3. **Cosmos DB Emulator**: Para desarrollo local, usar el emulador de Cosmos DB o permitir fallback a memoria.

4. **CORS**: Configurado para permitir puertos 3000, 3001, 3002 y 5173 (Vite).

5. **Moneda**: Sistema configurado para pesos chilenos (CLP) con precio de $1000 por foto.

## 🔜 Mejoras Futuras Sugeridas
- [ ] Implementar pasarela de pago (Stripe, PayPal, Mercado Pago)
- [ ] Panel de analytics y reportes
- [ ] Notificaciones push en tiempo real
- [ ] Sistema de cupones y descuentos
- [ ] Integración con múltiples proveedores de almacenamiento
- [ ] Tests automatizados (unitarios e integración)
- [ ] Cache con Redis para mejorar performance
- [ ] Compresión de imágenes automática
- [ ] Búsqueda avanzada de fotos por metadatos