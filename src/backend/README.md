# PhotosMarket API - Backend

API REST para PhotosMarket, una aplicación que permite a los clientes comprar fotografías de alta resolución desde Google Photos del fotógrafo.

## Stack Tecnológico

- **.NET 8.0** - Framework principal
- **Azure Cosmos DB** - Base de datos NoSQL
- **Google Photos API** - Integración con Google Photos
- **OAuth 2.0** - Autenticación con Google
- **JWT** - Autenticación de usuarios
- **MailKit** - Envío de correos electrónicos

## Requisitos Previos

1. .NET 8.0 SDK
2. Cuenta Azure con Cosmos DB
3. Proyecto Google Cloud con Photos Library API habilitada
4. Credenciales OAuth 2.0 de Google

## Configuración Inicial

### 1. Clonar y Configurar

```bash
cd src/backend
cp appsettings.template.json appsettings.json
cp appsettings.template.json appsettings.Development.json
```

### 2. Configurar Google Cloud Console

1. Ve a [Google Cloud Console](https://console.cloud.google.com/)
2. Crea un nuevo proyecto o selecciona uno existente
3. Habilita la **Photos Library API**
4. Ve a "Credenciales" y crea credenciales OAuth 2.0:
   - Tipo de aplicación: Aplicación web
   - URIs de redireccionamiento autorizados: `https://localhost:7001/api/auth/google-callback`
5. Copia el **Client ID** y **Client Secret**

### 3. Configurar Azure Cosmos DB

1. Ve a [Azure Portal](https://portal.azure.com/)
2. Crea una cuenta de Cosmos DB (API: Core SQL)
3. Copia la **cadena de conexión** desde "Keys"

### 4. Configurar appsettings.json

Edita `appsettings.json` y reemplaza los valores:

```json
{
  "ConnectionStrings": {
    "CosmosDb": "TU-CADENA-DE-CONEXION-COSMOS"
  },
  "GooglePhotos": {
    "ClientId": "TU-CLIENT-ID.apps.googleusercontent.com",
    "ClientSecret": "TU-CLIENT-SECRET"
  },
  "Jwt": {
    "SecretKey": "TU-CLAVE-SECRETA-MINIMO-32-CARACTERES"
  },
  "Email": {
    "SenderEmail": "tu-email@gmail.com",
    "SenderPassword": "tu-app-password"
  }
}
```

### 5. Configurar Email (Gmail)

1. Ve a tu cuenta de Google
2. Habilita verificación en dos pasos
3. Genera una **contraseña de aplicación**
4. Usa esa contraseña en `Email.SenderPassword`

## Ejecutar la Aplicación

### Restaurar dependencias
```bash
dotnet restore
```

### Compilar el proyecto
```bash
dotnet build
```

### Ejecutar en modo desarrollo
```bash
dotnet run
```

La API estará disponible en: `https://localhost:7001`

### Swagger UI
Accede a la documentación interactiva: `https://localhost:7001/swagger`

## Estructura del Proyecto

```
PhotosMarket.API/
├── Configuration/          # Clases de configuración
├── Controllers/           # Controladores REST API
├── DTOs/                 # Data Transfer Objects
├── Models/               # Modelos de dominio
├── Repositories/         # Capa de acceso a datos
├── Services/             # Lógica de negocio
├── Program.cs            # Punto de entrada
└── appsettings.json      # Configuración
```

## Endpoints Principales

### Autenticación
- `GET /api/auth/google-login` - Obtener URL de autenticación
- `POST /api/auth/google-callback` - Callback OAuth
- `GET /api/auth/validate` - Validar token JWT

### Fotos
- `GET /api/photos/albums` - Listar álbumes
- `GET /api/photos/albums/{albumId}/photos` - Fotos de un álbum
- `GET /api/photos/{mediaItemId}` - Obtener foto específica

### Pedidos
- `POST /api/orders` - Crear pedido
- `GET /api/orders` - Listar pedidos del usuario
- `GET /api/orders/{orderId}` - Obtener pedido específico
- `POST /api/orders/{orderId}/confirm-payment` - Confirmar pago

### Descargas
- `GET /api/download/{token}` - Verificar link de descarga
- `GET /api/download/{token}/files` - Obtener URLs de fotos

## Flujo de Usuario

1. **Autenticación**: Usuario inicia sesión con Google
2. **Exploración**: Navega por álbumes y fotos (con marca de agua)
3. **Selección**: Agrega fotos al carrito
4. **Pedido**: Crea un pedido
5. **Email 1**: Recibe email con detalles de pago
6. **Pago**: Realiza transferencia electrónica
7. **Confirmación**: Admin confirma el pago
8. **Procesamiento**: Sistema genera link de descarga
9. **Email 2**: Recibe email con link de descarga
10. **Descarga**: Descarga fotos en alta resolución

## Variables de Entorno (Producción)

Para producción, usa variables de entorno en lugar de appsettings.json:

```bash
export ConnectionStrings__CosmosDb="..."
export GooglePhotos__ClientId="..."
export GooglePhotos__ClientSecret="..."
export Jwt__SecretKey="..."
export Email__SenderEmail="..."
export Email__SenderPassword="..."
```

## Seguridad

- ✅ Autenticación OAuth 2.0 con Google
- ✅ JWT para sesiones de usuario
- ✅ HTTPS obligatorio
- ✅ CORS configurado
- ✅ Validación de tokens
- ✅ Partition keys en Cosmos DB

## Soporte

Para preguntas o problemas, consulta la especificación en `specification/README.md`.
