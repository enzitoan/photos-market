# PhotosMarket Frontend

Frontend desarrollado en Vue 3 + Vite para el sistema de marketplace de fotografías.

## 📋 Requisitos Previos

- Node.js 18+ y npm/yarn
- Backend de PhotosMarket ejecutándose en http://localhost:5000

## 🚀 Instalación

```bash
# Instalar dependencias
npm install
```

## 🔧 Configuración

El frontend está configurado para conectarse al backend en `http://localhost:5000` mediante un proxy en Vite. No requiere configuración adicional para desarrollo local.

Si necesitas cambiar la URL del backend, edita `vite.config.js`:

```javascript
proxy: {
  '/api': {
    target: 'http://tu-backend-url:puerto',
    changeOrigin: true
  }
}
```

## 💻 Comandos de Desarrollo

```bash
# Modo desarrollo (hot-reload)
npm run dev

# Build para producción
npm run build

# Preview del build de producción
npm run preview
```

## 📁 Estructura del Proyecto

```
src/
├── components/          # Componentes reutilizables
│   ├── NavBar.vue
│   ├── PhotoCard.vue
│   ├── AlbumCard.vue
│   └── LoadingSpinner.vue
├── views/              # Vistas/páginas principales
│   ├── HomeView.vue
│   ├── AlbumsView.vue
│   ├── AlbumPhotosView.vue
│   ├── CartView.vue
│   ├── OrdersView.vue
│   ├── DownloadView.vue
│   ├── auth/
│   │   ├── LoginView.vue
│   │   └── CallbackView.vue
│   └── admin/
│       ├── AdminLayout.vue
│       ├── DashboardView.vue
│       ├── AlbumsManagementView.vue
│       ├── OrdersManagementView.vue
│       └── SettingsView.vue
├── stores/             # Stores de Pinia (state management)
│   ├── auth.js
│   ├── cart.js
│   └── admin.js
├── services/           # Servicios de API
│   ├── httpClient.js
│   ├── authService.js
│   ├── photosService.js
│   ├── ordersService.js
│   └── downloadService.js
├── router/             # Configuración de rutas
│   └── index.js
├── main.js            # Entry point
└── style.css          # Estilos globales
```

## 🎨 Características Principales

### Para Clientes
- ✅ Autenticación con Google OAuth 2.0
- ✅ Exploración de álbumes y fotos
- ✅ Carrito de compras con gestión de fotos
- ✅ Sistema de pedidos con seguimiento
- ✅ Descarga de fotos en alta resolución
- ✅ Watermark en fotos de vista previa

### Panel de Administración (Fotógrafo)
- ✅ Dashboard con estadísticas
- ✅ Gestión de pedidos (confirmar pago/cancelar)
- ✅ Bloqueo/desbloqueo de álbumes
- ✅ Configuración de marca de agua
- ✅ Configuración de precios

## 🔐 Rutas de la Aplicación

### Públicas
- `/` - Página de inicio
- `/login` - Inicio de sesión
- `/auth/callback` - OAuth callback

### Protegidas (requieren autenticación)
- `/albums` - Lista de álbumes
- `/albums/:id` - Fotos de un álbum
- `/cart` - Carrito de compras
- `/orders` - Historial de pedidos
- `/orders/:id` - Detalle de pedido
- `/download/:token` - Descarga de fotos

### Admin (requieren rol de administrador)
- `/admin/dashboard` - Panel principal
- `/admin/orders` - Gestión de pedidos
- `/admin/albums` - Gestión de álbumes
- `/admin/settings` - Configuración del sistema

## 🛠️ Tecnologías Utilizadas

- **Vue 3** - Framework progresivo de JavaScript
- **Vite** - Build tool y dev server
- **Vue Router 4** - Enrutamiento
- **Pinia** - State management
- **Axios** - Cliente HTTP
- **Tailwind CSS** - Framework CSS
- **vue-toastification** - Notificaciones toast

## 📦 State Management (Pinia)

### Auth Store
- Manejo de sesión de usuario
- Tokens JWT
- Estado de autenticación

### Cart Store
- Gestión del carrito de compras
- Persistencia en localStorage
- Cálculo de totales

### Admin Store
- Configuración de marca de agua
- Gestión de álbumes bloqueados
- Persistencia en localStorage

## 🔄 Flujo de Autenticación

1. Usuario hace clic en "Iniciar Sesión con Google"
2. Se redirige a Google OAuth
3. Google redirige a `/auth/callback?code=...`
4. El frontend envía el código al backend
5. Backend valida y retorna JWT
6. JWT se almacena y se usa en requests subsiguientes

## 🎯 Próximos Pasos para Producción

1. Configurar variables de entorno para la URL del backend
2. Optimizar imágenes y assets
3. Configurar CDN para distribución de assets
4. Implementar lazy loading adicional para rutas
5. Configurar analytics (Google Analytics, etc.)
6. Implementar error tracking (Sentry, etc.)
7. Configurar PWA para offline support

## 📝 Notas de Desarrollo

- El proxy de Vite solo funciona en desarrollo. En producción, necesitarás configurar CORS en el backend.
- Los stores de Pinia persisten datos en localStorage para mejor UX.
- Las rutas de admin están protegidas por guards que verifican el rol del usuario.
- Los tokens JWT se renuevan automáticamente mediante interceptors de Axios.

## 🐛 Debugging

Para debugging, abre las DevTools de Vue:
- Instala Vue DevTools extension para Chrome/Firefox
- Inspecciona stores, rutas y componentes en tiempo real

## 📄 Licencia

Este proyecto forma parte del sistema PhotosMarket.
