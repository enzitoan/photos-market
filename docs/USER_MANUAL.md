# 📚 Manual de Usuario - PhotosMarket

Guía completa para configurar, usar y administrar el sistema PhotosMarket.

## 📋 Tabla de Contenidos

- [Configuración Inicial](#-configuración-inicial)
- [Guía de Cliente](#-guía-de-cliente)
- [Guía de Administrador](#-guía-de-administrador)
- [Casos de Uso Comunes](#-casos-de-uso-comunes)
- [Seguridad y Buenas Prácticas](#-seguridad-y-buenas-prácticas)
- [Troubleshooting](#-troubleshooting)

---

## 🔐 Configuración Inicial

### 1. Crear Proyecto en Google Cloud

1. Ir a [Google Cloud Console](https://console.cloud.google.com/)
2. Crear nuevo proyecto o seleccionar uno existente
3. Habilitar **Google Drive API**:
   - Ir a "APIs & Services" → "Library"
   - Buscar "Google Drive API"
   - Hacer clic en "Enable"

### 2. Configurar OAuth 2.0

1. En Google Cloud Console, ir a "APIs & Services" → "Credentials"
2. Hacer clic en "Create Credentials" → "OAuth client ID"
3. Tipo de aplicación: **Web application**
4. Configurar:
   - **Name**: PhotosMarket
   - **Authorized redirect URIs**:
     - `http://localhost:3001/callback` (desarrollo)
     - `https://tu-dominio.com/callback` (producción)
5. Guardar **Client ID** y **Client Secret**

### 3. Crear Credenciales de Service Account

1. En "Credentials", hacer clic en "Create Credentials" → "Service account"
2. Completar información básica
3. Descargar el archivo JSON de credenciales
4. Renombrar a `google-drive-credentials.json`
5. Colocar en `src/backend/google-drive-credentials.json`

### 4. Configurar Google Drive

1. Crear carpeta raíz en Google Drive para las fotos
2. Compartir la carpeta con el email del Service Account
3. Copiar el **Folder ID** de la URL:
   - URL example: `https://drive.google.com/drive/folders/1ABC...XYZ`
   - Folder ID: `1ABC...XYZ`

### 5. Configurar Variables de Entorno

#### Para Desarrollo Local

Editar `src/backend/appsettings.json`:

```json
{
  "GoogleOAuth": {
    "ClientId": "TU_CLIENT_ID.apps.googleusercontent.com",
    "ClientSecret": "TU_CLIENT_SECRET",
    "RedirectUri": "http://localhost:3001/callback"
  },
  "GoogleDrive": {
    "RootFolderId": "TU_FOLDER_ID_DE_GOOGLE_DRIVE",
    "CredentialsFilePath": "google-drive-credentials.json",
    "ApplicationName": "PhotosMarket"
  },
  "Jwt": {
    "SecretKey": "minimo-32-caracteres-super-secreto-para-seguridad",
    "Issuer": "PhotosMarketAPI",
    "Audience": "PhotosMarketClient",
    "ExpirationInMinutes": 60
  },
  "Email": {
    "Enabled": true,
    "ApiKey": "re_TU_RESEND_API_KEY",
    "SenderEmail": "noreply@tu-dominio.com",
    "SenderName": "PhotosMarket"
  },
  "Application": {
    "BaseUrl": "http://localhost:5000",
    "FrontendUrl": "http://localhost:3001",
    "DownloadLinkExpirationHours": 72,
    "DefaultWatermarkText": "@egan.fotografia",
    "PhotoPricePerUnit": 1000,
    "Currency": "CLP",
    "BulkDiscountMinPhotos": 5,
    "BulkDiscountPercentage": 20,
    "WatermarkFontSizeDivisor": 30,
    "WatermarkTextOpacity": 0.8,
    "WatermarkShadowOpacity": 0.7,
    "WatermarkVerticalPosition": 0.9
  }
}
```

#### Para Docker

Editar `.env`:

```bash
GOOGLE_OAUTH_CLIENT_ID=TU_CLIENT_ID
GOOGLE_OAUTH_CLIENT_SECRET=TU_CLIENT_SECRET
GOOGLE_DRIVE_ROOT_FOLDER_ID=TU_FOLDER_ID
JWT_SECRET_KEY=tu-secreto-de-32-caracteres-minimo
EMAIL_API_KEY=re_TU_RESEND_API_KEY
```

---

## 👤 Guía de Cliente

### 1. Registro e Inicio de Sesión

#### Primer Acceso

1. Acceder a la aplicación (desarrollo: http://localhost:3001)
2. En la página de inicio, hacer clic en **"Continuar con Google"**
3. Seleccionar tu cuenta de Google o crear una nueva
4. Autorizar los permisos solicitados por la aplicación:
   - Ver tu información básica de perfil
   - Ver tu dirección de correo electrónico
5. Serás redirigido automáticamente a la página de álbumes

#### Sesiones Posteriores

- Tu sesión permanece activa por 60 minutos
- Si expira, simplemente vuelve a iniciar sesión con Google
- Tus datos y pedidos anteriores se mantienen intactos

### 2. Explorar Álbumes

#### Vista Principal de Álbumes

- Al iniciar sesión, verás una galería con todos los álbumes disponibles
- Cada álbum muestra:
  - Nombre del álbum
  - Foto de portada (primera foto del álbum)
  - Cantidad de fotos en el álbum
  - Estado (Disponible/Bloqueado)

#### Filtrado y Navegación

- Los álbumes bloqueados por el administrador no son visibles
- Hacer clic en cualquier álbum para ver sus fotos
- Las miniaturas tienen marca de agua CSS (overlay) para protección

### 3. Seleccionar Fotos

#### Visualizar Fotos

1. Hacer clic en un álbum para entrar
2. Ver galería de miniaturas (400x400px) con marca de agua
3. Hacer clic en cualquier foto para ver en tamaño grande (modal)
4. En el modal puedes:
   - Ver la foto en mayor detalle
   - Agregar/quitar del carrito
   - Navegar entre fotos con flechas ← →

#### Agregar al Carrito

1. En la vista de galería o en el modal, hacer clic en **"Agregar al Carrito"** (icono 🛒)
2. El botón cambia a **"Quitar del Carrito"** si ya está agregada
3. El contador del carrito en la barra superior se actualiza automáticamente
4. Puedes agregar fotos de múltiples álbumes

#### Gestionar Carrito

- El ícono del carrito muestra el número total de fotos seleccionadas
- Hacer clic en el ícono del carrito para ver el resumen
- En el carrito puedes:
  - Ver todas las fotos seleccionadas con miniaturas
  - Eliminar fotos individuales
  - Ver precio unitario y subtotal
  - Ver descuentos aplicados automáticamente

### 4. Sistema de Precios y Descuentos

#### Precio Base

- **Precio por foto**: $1,000 CLP (configurable por el fotógrafo)
- Precio mostrado en pesos chilenos (CLP)

#### Descuentos Automáticos

- **Descuento por volumen**: 20% de descuento al comprar 5 o más fotos
- Ejemplo de cálculo:
  - 4 fotos → $4,000 (sin descuento)
  - 5 fotos → Subtotal $5,000 → Descuento 20% = -$1,000 → **Total $4,000**
  - 10 fotos → Subtotal $10,000 → Descuento 20% = -$2,000 → **Total $8,000**

#### Visualización en Carrito

El carrito muestra:
- Cantidad de fotos seleccionadas
- Precio unitario
- Subtotal (cantidad × precio)
- Descuento aplicado (si corresponde)
- **Total a pagar**

### 5. Crear Pedido

#### Proceso de Creación

1. Revisar el carrito con todas tus fotos seleccionadas
2. Verificar el precio total
3. Hacer clic en **"Crear Pedido"**
4. El sistema genera automáticamente:
   - **Número de pedido único** (ej: ORD-20260331-ABC123)
   - Estado inicial: "Esperando Pago"

#### Confirmación por Email

Recibirás inmediatamente un email con:
- ✅ Número de pedido
- ✅ Lista de fotos seleccionadas (con miniaturas)
- ✅ Precio total y descuentos aplicados
- ✅ Instrucciones de pago
- ✅ Datos bancarios para transferencia

### 6. Realizar Pago

#### Método de Pago

El sistema opera con **transferencia bancaria**:

1. Realizar transferencia por el monto exacto indicado
2. En el campo "Mensaje" o "Referencia", incluir tu **número de pedido**
3. Notificar al fotógrafo que realizaste el pago (WhatsApp, email, etc.)
4. Esperar confirmación del fotógrafo

#### Importante

- No enviar capturas de pantalla por el sistema
- El fotógrafo verificará el pago en su cuenta bancaria
- La confirmación puede tomar hasta 24 horas

### 7. Seguimiento de Pedidos

#### Ver Mis Pedidos

1. Hacer clic en **"Mis Pedidos"** en el menú principal
2. Ver lista de todos tus pedidos con:
   - Número de pedido
   - Fecha de creación
   - Cantidad de fotos
   - Precio total
   - Estado actual

#### Estados de Pedido

- 🟡 **Pending** (Esperando Pago): Transferencia pendiente
- 🟢 **PaymentConfirmed** (Pago Confirmado): Pago verificado, preparando descarga
- ✅ **Completed** (Completado): Listo para descargar
- ❌ **Cancelled** (Cancelado): Pedido cancelado

#### Detalle de Pedido

Hacer clic en cualquier pedido para ver:
- Información completa del pedido
- Galería de fotos incluidas
- Historial de cambios de estado
- Botón de descarga (si está disponible)

### 8. Descargar Fotos

#### Esperar Confirmación

Después de realizar el pago:

1. **Email 1**: Confirmación de orden creada (inmediato)
2. **Email 2**: Pago confirmado por el fotógrafo (al verificar transferencia)
3. **Email 3**: Enlace de descarga listo (al marcar pedido como completado)

#### Proceso de Descarga

**Opción A: Desde el Email**

1. Revisa tu bandeja de entrada (y spam)
2. Busca el email "Enlace de Descarga Disponible"
3. Hacer clic en el enlace único del email
4. Serás redirigido a la página de descarga

**Opción B: Desde el Panel**

1. Ir a **"Mis Pedidos"**
2. Hacer clic en el pedido completado
3. Hacer clic en **"Ver Descargas"**
4. Ver galería de tus fotos compradas

#### Descargar Archivos

1. En la página de descarga, verás miniaturas de todas tus fotos
2. Hacer clic en **"⬇️ Descargar"** en cada foto que desees
3. Las fotos se descargan individualmente al hacer clic
4. Cada foto descargada incluye:
   - ✅ **Resolución original completa** (sin comprimir)
   - ✅ **Marca de agua permanente** `@egan.fotografia` en centro/inferior
   - ✅ **Metadatos EXIF preservados** (fecha, cámara, configuración)
   - ✅ **Formato original** (JPG, PNG, etc.)

#### Características de la Marca de Agua

La marca de agua en las fotos descargadas:
- **Es permanente** (renderizada en la imagen, no removible)
- **No afecta la calidad** de la foto
- **Protege los derechos** del fotógrafo
- **Personalizada** con el texto configurado por el fotógrafo
- **Posicionada** estratégicamente (usualmente centro/inferior)

#### ⚠️ Importante: Vigencia del Enlace

- El enlace de descarga **expira en 72 horas** (3 días)
- Después de la expiración, el enlace deja de funcionar
- Descarga todas tus fotos dentro del plazo
- Si expira, contacta al fotógrafo para generar nuevo enlace

#### Consejos para Descargar

✅ **Recomendado**:
- Descargar todas las fotos inmediatamente al recibir el enlace
- Guardar las fotos en una ubicación segura (disco duro, nube personal)
- Verificar que todas las fotos se descargaron correctamente

❌ **Evitar**:
- Esperar hasta el último día
- Descargar desde conexiones lentas o inestables
- Cerrar el navegador antes de completar todas las descargas

---

## 👨‍💼 Guía de Administrador

### 1. Acceso al Panel de Administración

#### Primera Vez

1. Abrir navegador y navegar a: `http://localhost:3001/admin` (o tu dominio en producción)
2. Hacer clic en **"Iniciar Sesión como Administrador"**
3. Usar cuenta autorizada de Google (configurada en el sistema):
   - Email por defecto: `ahumada.enzo@gmail.com`
4. Completar autenticación con Google OAuth
5. Serás redirigido al Dashboard de administración

#### Sesiones Posteriores

- La sesión admin permanece activa por 60 minutos
- Después de expirar, vuelve a autenticarte
- Los datos del sistema se mantienen intactos

### 2. Dashboard Principal

#### Vista General

Al acceder al panel, verás el dashboard con:

- **Total de Pedidos**: Cantidad acumulada de todos los tiempos
- **Ingresos Totales**: Suma de todos los pedidos completados (en CLP)
- **Pedidos Pendientes**: Cantidad esperando confirmación de pago
- **Pedidos Completados**: Cantidad finalizada y descargada

#### Gráficos y Reportes

- **Pedidos por Estado**: Distribución visual (torta/barras)
- **Fotos Más Vendidas**: Top 10 de imágenes populares
- **Tendencias de Ventas**: Evolución temporal de pedidos e ingresos
- **Álbumes Más Populares**: Rankings de colecciones

### 3. Autenticación con Google Drive

#### Configuración Inicial (Primera Vez)

1. En el panel admin, ir a **"Configuración"** en el menú lateral
2. Buscar la sección "Google Drive Integration"
3. Hacer clic en **"Autenticar Google Drive"**
4. Serás redirigido a Google para autorizar
5. Conceder permisos de acceso a Google Drive
6. Confirmar acceso exitoso con mensaje verde

#### Verificar Conexión

- Estado mostrado en la sección de configuración
- Ícono verde: Conectado ✅
- Ícono rojo: Desconectado ❌
- Botón para re-autenticar si es necesario

### 4. Gestión de Álbumes

#### Acceder a Gestión

1. En el menú lateral, hacer clic en **"Gestión de Álbumes"**
2. Ver lista completa de álbumes sincronizados desde Google Drive

#### Información de Álbumes

Cada álbum muestra:
- **Nombre**: Nombre de la carpeta en Google Drive
- **Cantidad de Fotos**: Total de imágenes en el álbum
- **Estado**: Bloqueado/Desbloqueado
- **Fecha de Sincronización**: Última actualización

#### Bloquear/Desbloquear Álbumes

**Bloquear un Álbum**:
1. Encontrar el álbum en la lista
2. Hacer clic en el interruptor de estado (toggle)
3. Confirmar el cambio
4. El álbum **desaparece de la vista de clientes** inmediatamente

**Desbloquear un Álbum**:
1. Ubicar el álbum bloqueado (marcado con candado 🔒)
2. Hacer clic en el interruptor para desbloquear
3. El álbum **vuelve a ser visible** para clientes

#### Casos de Uso para Bloqueo

Bloquear álbumes cuando:
- ✅ Fotos aún no están listas para venta
- ✅ Álbum contiene fotos privadas o de prueba
- ✅ Cliente solicitó remover temporalmente
- ✅ Álbum necesita revisión o edición

#### Sincronización

- Los álbumes se sincronizan automáticamente desde Google Drive
- Nuevas carpetas aparecen automáticamente
- Fotos nuevas se detectan en tiempo real
- No requiere acción manual de sincronización

### 5. Gestión de Pedidos

#### Vista de Pedidos

1. Ir a **"Gestión de Pedidos"** en el menú
2. Ver tabla completa con todos los pedidos del sistema

#### Columnas de Información

- **ID de Pedido**: Identificador único (ej: ORD-20260331-XYZ)
- **Cliente**: Email del usuario
- **Fecha**: Fecha y hora de creación
- **Fotos**: Cantidad de imágenes en el pedido
- **Total**: Precio total (incluyendo descuentos)
- **Estado**: Estado actual del pedido
- **Acciones**: Botones de acción disponibles

#### Estados de Pedidos

| Estado | Icono | Descripción | Acciones Disponibles |
|--------|-------|-------------|---------------------|
| **Pending** | 🟡 | Esperando pago | Confirmar Pago, Cancelar |
| **PaymentConfirmed** | 🟢 | Pago verificado | Marcar Completado, Cancelar |
| **Completed** | ✅ | Descarga disponible | Ver Detalles |
| **Cancelled** | ❌ | Pedido cancelado | Ver Detalles |

#### Confirmar Pago Recibido

**Flujo Completo**:

1. **Cliente te notifica** que realizó la transferencia:
   - Por WhatsApp, email, o mensaje directo
   - Con número de pedido y monto transferido

2. **Verificar en tu cuenta bancaria**:
   - Buscar la transferencia por el monto exacto
   - Verificar que el mensaje/referencia contiene el número de pedido
   - Confirmar que el monto coincide con el pedido

3. **Confirmar en el sistema**:
   - En "Gestión de Pedidos", buscar el pedido (estado "Pending")
   - Hacer clic en **"Confirmar Pago"**
   - El sistema **automáticamente**:
     - ✅ Cambia estado a "PaymentConfirmed"
     - ✅ Envía email de confirmación al cliente
     - ✅ Registra fecha y hora de confirmación
     - ✅ Notifica que recibirá enlace próximamente

4. **El cliente recibe email** con:
   - Confirmación de pago recibido
   - Resumen del pedido
   - Mensaje de que recibirá el enlace de descarga pronto

#### Completar Orden y Generar Enlace

**Proceso de Completar**:

1. **Después de confirmar el pago**, hacer clic en **"Marcar Completado"**

2. **El sistema automáticamente**:
   - ✅ Genera enlace de descarga único y seguro
   - ✅ Establece fecha de expiración (72 horas)
   - ✅ Cambia estado a "Completed"
   - ✅ **Envía email al cliente** con:
     - Enlace directo de descarga
     - Instrucciones de uso
     - Fecha de expiración del enlace
     - Lista de fotos disponibles

3. **El cliente puede descargar** inmediatamente:
   - Hacer clic en el enlace del email
   - O acceder desde "Mis Pedidos" en su panel
   - Descargar todas sus fotos en alta resolución

#### Sistema de Emails Automáticos

El sistema envía **3 emails** en cada ciclo de pedido:

| Evento | Template | Cuándo | Contenido |
|--------|----------|--------|-----------|
| **Orden Creada** | `order-created` | Al crear pedido | Número de orden, fotos, total, instrucciones de pago |
| **Pago Confirmado** | `payment-confirmed` | Al confirmar pago | Confirmación, resumen, aviso de enlace próximo |
| **Orden Completa** | `download-ready` | Al completar orden | Enlace de descarga, expiración, instrucciones |

#### Cancelar Pedido

**Cuándo cancelar**:
- Cliente solicitó cancelación
- Pago no fue recibido después de mucho tiempo
- Error en el pedido

**Proceso**:
1. Encontrar el pedido en la lista
2. Hacer clic en **"Cancelar"**
3. Confirmar la acción
4. Estado cambia a "Cancelled"
5. (Opcional) Contactar al cliente para notificar

### 6. Configuración del Sistema

#### Acceder a Configuración

1. Ir a **"Configuración"** en el menú lateral
2. Ver todas las opciones configurables del sistema

#### Secciones de Configuración

##### A) Precios y Moneda

```
Precio por Foto: $1,000
Moneda: CLP (Peso Chileno)
Descuento Mínimo: 5 fotos
Porcentaje de Descuento: 20%
```

**Modificar Precios**:
1. Ingresar nuevo valor en el campo
2. Hacer clic en **"Guardar Cambios"**
3. Los nuevos pedidos usarán el precio actualizado
4. **Pedidos existentes no se modifican**

##### B) Marca de Agua

```
Texto de Marca de Agua: @egan.fotografia
Tamaño (Divisor de Fuente): 30
Opacidad del Texto: 0.8
Opacidad de la Sombra: 0.7
Posición Vertical: 0.9
```

**Personalizar Marca de Agua**:
- **Texto**: Cambia el texto que aparece en las fotos descargadas
- **Tamaño**: Menor número = marca más grande (rango: 20-50)
  - 20 = Muy grande
  - 30 = Mediano (default)
  - 40 = Pequeño
- **Opacidad Texto**: Transparencia del texto blanco (0.0-1.0)
  - 0.6 = Sutil
  - 0.8 = Balanceado (default)
  - 1.0 = Completamente opaco
- **Opacidad Sombra**: Transparencia de la sombra negra (0.0-1.0)
- **Posición Vertical**: Ubicación en el eje Y (0.0-1.0)
  - 0.0 = Arriba
  - 0.5 = Centro
  - 0.9 = Inferior (default)

##### C) Enlaces de Descarga

```
Tiempo de Expiración: 72 horas
```

**Modificar Expiración**:
- Cambiar el valor en horas (mínimo: 24, máximo: 168)
- Aplicable a nuevos enlaces generados
- Enlaces existentes mantienen su expiración original

##### D) Email

```
Email Habilitado: Sí/No
Proveedor: Resend
API Key: re_***************
Email Remitente: noreply@tu-dominio.com
Nombre Remitente: PhotosMarket
```

**en Azure con App Configuration**:
- Todos estos valores podrían migrarse a **Azure App Configuration**
- Permitiría cambiar configuración sin redesplegar
- Feature flags para activar/desactivar emails dinámicamente
- Gestión centralizada multi-entorno (dev/staging/prod)

##### E) Google Drive

```
Carpeta Raíz ID: 1ABC...XYZ
Estado de Conexión: ✅ Conectado / ❌ Desconectado
```

**Re-autenticar**:
- Si la conexión falla, hacer clic en "Autenticar Google Drive"
- Volver a autorizar permisos
- Verificar que el Service Account tiene acceso a la carpeta

### 7. Casos de Uso Avanzados

#### Agregar Nuevas Fotos al Sistema

1. **En Google Drive**:
   - Abrir la carpeta raíz configurada
   - Crear una nueva subcarpeta (nombre = nombre del álbum)
   - Subir fotos a la subcarpeta
   - Asegurarse de que las fotos estén en formato JPG, PNG o similar

2. **En el Sistema**:
   - El álbum aparece automáticamente en "Gestión de Álbumes"
   - Por defecto está **desbloqueado** (visible para clientes)
   - Si deseas revisar antes de publicar, bloquearlo inmediatamente

3. **Publicar**:
   - Verificar que todas las fotos se ven correctamente
   - Asegurarse de que el álbum esté desbloqueado
   - Los clientes verán el álbum inmediatamente

#### Regenerar Enlace de Descarga Expirado

**Escenario**: Cliente no descargó a tiempo y el enlace expiró (72 horas).

**Solución Actual**:
1. Ir a "Gestión de Pedidos"
2. Encontrar el pedido del cliente (estado "Completed")
3. **Opción temporal**:
   - Cambiar estado a "PaymentConfirmed"
   - Volver a hacer clic en "Marcar Completado"
   - Se genera nuevo enlace y email

**Mejora Futura** (pendiente):
- Botón específico "Regenerar Enlace"
- Sin cambiar el estado del pedido
- Historial de enlaces generados

#### Modificar Pedido Existente

**Si cliente necesita agregar más fotos**:

1. **Crear nuevo pedido** para las fotos adicionales
2. Aplicar descuento manual si corresponde
3. Procesar el pago adicional
4. Generar enlace combinado o separado

**Importante**: No se puede modificar un pedido ya creado (integridad de datos)

#### Gestionar Múltiples Pedidos del Mismo Cliente

1. En "Gestión de Pedidos", filtrar por email del cliente
2. Ver historial completo de compras
3. Procesar cada pedido independientemente
4. Los enlaces de descarga son únicos por pedido

---

## 🔧 Casos de Uso Comunes

### Cliente No Recibe Emails

#### Sistema de 3 Emails Automáticos

El sistema envía automáticamente:
1. **Confirmación de Pedido** → Al crear la orden
2. **Pago Confirmado** → Al validar la transferencia
3. **Enlace de Descarga** → Al completar la orden

#### Troubleshooting de Emails

**Paso 1: Verificar con el Cliente**
- ¿Revisó la carpeta de spam/correo no deseado?
- ¿El email en su perfil es correcto?
- ¿Usa Gmail, Outlook u otro proveedor?

**Paso 2: Verificar en el Sistema**
1. Ir al pedido del cliente
2. Verificar que el email del usuario sea correcto
3. Revisar el estado del pedido (debe haber progresado correctamente)

**Paso 3: Revisar Logs del Backend**
```powershell
# Ver logs del backend
cd C:\repos\microsoft\photos-market\src\backend
dotnet run

# Buscar errores de email en la consola
# Buscar líneas con "Email" o "Resend"
```

**Paso 4: Verificar Configuración de Resend**

En `appsettings.json`:
```json
{
  "Email": {
    "Enabled": true,  // ¿Está habilitado?
    "ApiKey": "re_...",  // ¿API Key válida?
    "SenderEmail": "noreply@tu-dominio.com",  // ¿Email verificado en Resend?
    "SenderName": "PhotosMarket"
  }
}
```

**Verificar en Resend Dashboard**:
1. Ir a https://resend.com/emails
2. Ver lista de emails enviados
3. Verificar estado (Delivered, Bounced, Rejected)
4. Ver razón de fallo si aplicable

**Paso 5: Si Usas SMTP/Gmail**

Si `Provider: "Smtp"` en configuración:
```json
{
  "Email": {
    "Provider": "Smtp",
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderPassword": "DEBE_SER_APP_PASSWORD",  // ⚠️ NO la contraseña normal
    "EnableSsl": true
  }
}
```

**Generar App Password en Gmail**:
1. Ir a cuenta Google → Seguridad
2. Activar Verificación en 2 pasos
3. Ir a "Contraseñas de aplicaciones"
4. Generar nueva contraseña
5. Usar esa contraseña (sin espacios) en `SenderPassword`

**Paso 6: Solución Alternativa**

Si los emails no funcionan:
1. El cliente puede acceder a "Mis Pedidos"
2. Ver el estado actual del pedido
3. Acceder al enlace de descarga directamente desde ahí
4. No depende del email para descargar

### Enlace de Descarga Expirado

#### Comprensión del Problema

- Los enlaces expiran automáticamente después de **72 horas** (3 días)
- Es una medida de seguridad para proteger las fotos
- Después de la expiración, el token de descarga es inválido

#### Detectar Enlace Expirado

El cliente verá:
- Mensaje de error: "El enlace de descarga ha expirado"
- Código de estado HTTP 410 (Gone)
- No puede acceder a las fotos

#### Solución

**Opción 1: Regenerar Manualmente** (temporal)
1. Ir a "Gestión de Pedidos"
2. Localizar el pedido del cliente
3. Cambiar estado de "Completed" a "PaymentConfirmed"
4. Guardar el cambio
5. Volver a hacer clic en "Marcar Completado"
6. Se genera nuevo enlace automáticamente
7. Cliente recibe nuevo email con enlace fresco

**Opción 2: Extender Plazo de Expiración** (preventivo)
1. Ir a "Configuración"
2. Modificar "Tiempo de Expiración de Enlaces"
3. Cambiar de 72 horas a 168 horas (7 días) o más
4. Guardar cambios
5. **Nota**: Solo afecta nuevos enlaces generados

**Mejora Futura**:
- Implementar botón "Regenerar Enlace" específico
- Permitir extensiones de tiempo individual sin cambiar estado
- Notificación automática al cliente antes de expirar

### Modificar Precios

#### Cambiar Precio por Foto

1. Ir a **"Configuración"** → Sección "Precios"
2. Modificar campo "Precio por Foto"
3. Cambiar de $1,000 a (ej: $1,500)
4. Hacer clic en **"Guardar Cambios"**
5. Confirmar el cambio

#### Impacto del Cambio

✅ **Nuevos Pedidos**:
- Usarán el precio actualizado inmediatamente
- Descuentos se calculan con el nuevo precio

❌ **Pedidos Existentes**:
- Mantienen su precio original
- No se modifican retroactivamente
- Garantiza integridad de transacciones completadas

#### Modificar Descuentos

**Cambiar Descuento Mínimo**:
```
Descuento Mínimo: 5 → 10 fotos
```
Ahora se requieren 10 fotos para activar el descuento.

**Cambiar Porcentaje**:
```
Porcentaje de Descuento: 20% → 15%
```
Ahora el descuento es de 15% en lugar de 20%.

**Ejemplos de Cálculo**:
- 10 fotos a $1,000 con 15% descuento:
  - Subtotal: $10,000
  - Descuento: -$1,500
  - **Total: $8,500**

### Bloquear Álbum Temporalmente

#### Cuándo Bloquear

Bloquear un álbum cuando:
- Estás editando o subiendo fotos nuevas
- El cliente solicitó privacidad temporal
- Álbum contiene fotos sensibles o de prueba
- Quieres limitar la venta a álbumes específicos

#### Proceso de Bloqueo

1. Ir a **"Gestión de Álbumes"**
2. Localizar el álbum en la lista
3. Hacer clic en el toggle/interruptor de estado
4. El álbum muestra ícono de candado 🔒
5. **Efecto inmediato**: Los clientes ya no lo ven

#### Verificar que Está Bloqueado

**Como Admin**:
- El álbum aparece con indicador "Bloqueado"
- Background gris o ícono de candado

**Como Cliente** (prueba en ventana incógnita):
- El álbum no aparece en la lista de álbumes
- No es accesible mediante URL directa

#### Desbloquear

1. En "Gestión de Álbumes"
2. Localizar el álbum bloqueado
3. Hacer clic nuevamente en el toggle
4. **Efecto inmediato**: Vuelve a ser visible para clientes

### Gestionar Reembolsos

#### Escenario: Cliente Solicita Reembolso

**Si el pedido aún no fue procesado**:
1. Ir a "Gestión de Pedidos"
2. Encontrar el pedido del cliente
3. Hacer clic en **"Cancelar"**
4. Procesar reembolso bancario manualmente
5. Notificar al cliente

**Si el pedido ya fue completado**:
- Las fotos ya fueron descargadas
- Evaluar según tu política de reembolsos
- Considerar devolución parcial si corresponde

---

## 🛡️ Seguridad y Buenas Prácticas

### Gestión de Credenciales

#### Reglas de Oro

1. ✅ **NUNCA** commitar credenciales en Git
2. ✅ **SIEMPRE** usar variables de entorno para secretos
3. ✅ **NUNCA** compartir `appsettings.json` con valores reales
4. ✅ **USAR** `appsettings.template.json` como referencia
5. ✅ **AGREGAR** `appsettings.json` al `.gitignore`

#### Archivos Sensibles

**Proteger estos archivos**:
```
src/backend/appsettings.json
src/backend/google-drive-credentials.json
.env
```

**Verificar .gitignore**:
```gitignore
appsettings.json
google-drive-credentials.json
.env
*.user
```

### Seguridad en Producción

#### HTTPS Obligatorio

En producción, **siempre usar HTTPS**:
- Azure Container Apps habilita HTTPS automáticamente
- Redirigir HTTP → HTTPS en la configuración
- Nunca transmitir tokens por HTTP

#### Rotar Secretos Periódicamente

**JWT Secret Key**:
- Cambiar cada 3-6 meses
- Generar con: `openssl rand -base64 64`
- Al cambiar, los tokens existentes se invalidan (usuarios deben re-autenticar)

**Google OAuth Credentials**:
- Rotar si hay sospecha de compromiso
- Actualizar en Google Cloud Console
- Sincronizar con Azure Key Vault

**Email API Keys**:
- Monitorear uso en Resend Dashboard
- Rotar si se detecta uso anormal
- Actualizar en configuración y Key Vault

### Monitoreo y Auditoría

#### Azure Log Analytics

Monitorear en producción:
1. Ir a Azure Portal
2. Abrir Log Analytics Workspace
3. Ejecutar queries KQL para análisis

**Queries Útiles**:

```kql
// Errores recientes
ContainerAppConsoleLogs_CL
| where Log_s contains "error" or Log_s contains "exception"
| order by TimeGenerated desc
| take 100

// Pedidos creados hoy
ContainerAppConsoleLogs_CL
| where Log_s contains "Order created"
| where TimeGenerated > ago(1d)
| summarize count()

// Autenticaciones fallidas
ContainerAppConsoleLogs_CL
| where Log_s contains "Authentication failed"
| order by TimeGenerated desc
```

#### Logs Locales

En desarrollo:
```powershell
# Ver logs del backend en tiempo real
cd src/backend
dotnet run

# Los logs aparecen en la consola con niveles:
# [Information] - Operaciones normales
# [Warning] - Situaciones anormales pero manejables
# [Error] - Errores que requieren atención
```

### Backups

#### Azure Cosmos DB

**Configurar Backup Automático**:
1. Azure Portal → Tu Cosmos DB Account
2. Ir a "Backup & Restore"
3. Configurar:
   - **Modo**: Continuous (7-30 días de retención)
   - **Intervalo**: Cada 4 horas
   - **Retención**: 30 días

**Restaurar desde Backup**:
1. En caso de pérdida de datos
2. Azure Portal → "Backup & Restore" → "Restore"
3. Seleccionar punto en el tiempo
4. Crear nueva cuenta o restaurar en la existente

#### Google Drive

- Las fotos están en Google Drive (no en el sistema)
- Google Drive tiene versionado automático
- Configurar plan de Google One si necesitas más espacio
- Considerar backup periódico a otro servicio

### Mejores Prácticas de Operación

#### 1. Actualizaciones Regulares

**Backend (.NET)**:
```powershell
# Verificar paquetes desactualizados
cd src/backend
dotnet list package --outdated

# Actualizar paquetes
dotnet add package Microsoft.EntityFrameworkCore --version x.x.x
```

**Frontend (Node.js)**:
```bash
# Auditoría de seguridad
cd src/frontend
npm audit

# Corregir vulnerabilidades
npm audit fix

# Actualizar dependencias
npm update
```

#### 2. Pruebas Antes de Producción

Antes de cada despliegue:
- ✅ Probar flujo completo cliente (login → compra → descarga)
- ✅ Probar flujo admin (gestión pedidos, confirmación pago)
- ✅ Verificar emails se envían correctamente
- ✅ Validar marca de agua en descargas
- ✅ Revisar logs por errores

#### 3. Comunicación con Clientes

- Responder rápidamente a notificaciones de pago
- Confirmar pedidos dentro de 24 horas
- Generar enlaces de descarga inmediatamente después de confirmar
- Recordar a clientes que descarguen antes de expiración

#### 4. Organización de Google Drive

- Mantener estructura clara de carpetas
- Nombres de álbumes descriptivos y sin caracteres especiales
- Evitar álbumes vacíos
- Comprimir fotos grandes antes de subir (opcional)

---

## 🐛 Troubleshooting

### Backend No Inicia

#### Síntoma
Al ejecutar `dotnet run`, el backend falla o no responde.

#### Diagnóstico

**1. Verificar Puerto Libre**:
```powershell
# Windows PowerShell
Get-NetTCPConnection -LocalPort 5000 -ErrorAction SilentlyContinue

# Si está ocupado, matar el proceso
Get-NetTCPConnection -LocalPort 5000 | 
  Select -ExpandProperty OwningProcess | 
  ForEach-Object { Stop-Process -Id $_ -Force }
```

**2. Verificar Instalación de .NET**:
```powershell
# Verificar versión (debe ser 8.0+)
dotnet --version

# Listar SDKs instalados
dotnet --list-sdks
```

**3. Limpiar y Reconstruir**:
```powershell
cd src/backend
dotnet clean
dotnet restore
dotnet build
```

**4. Revisar Errores de Compilación**:
```powershell
# Ver errores detallados
dotnet build --verbosity detailed
```

#### Soluciones Comunes

**Puerto en Uso**:
```powershell
# Cambiar puerto en launchSettings.json
# O ejecutar en puerto diferente:
dotnet run --urls "http://localhost:5001"
```

**falta appsettings.json**:
```powershell
cp appsettings.template.json appsettings.json
# Luego editar con tus credenciales
```

**Dependencias Corruptas**:
```powershell
Remove-Item -Recurse -Force bin, obj
dotnet restore
dotnet build
```

### Frontend No Inicia

#### Síntoma
`npm run dev` falla o muestra errores.

#### Diagnóstico

**1. Verificar Node.js**:
```bash
# Versión (debe ser 18+)
node --version

# Versión de npm
npm --version
```

**2. Limpiar node_modules**:
```bash
cd src/frontend
Remove-Item -Recurse -Force node_modules, package-lock.json
npm install
```

**3. Verificar Errores**:
```bash
npm run dev
# Leer mensajes de error en consola
```

#### Soluciones Comunes

**Dependencias Incompatibles**:
```bash
# Instalar con --force si hay conflictos
npm install --force
```

**Puerto 3001 en Uso**:
```bash
# El error mostrará el puerto alternativo
# O especificar puerto manualmente en vite.config.js
```

**Errores de ESLint**:
```bash
# Deshabilitar temporalmente
npm run dev -- --no-lint
```

**Cache de Vite Corrupto**:
```bash
# Limpiar cache
npm run dev -- --force --clear-screen
```

### Cosmos DB No Conecta

#### Síntoma
Backend inicia pero muestra errores al guardar datos.

#### Diagnóstico

**1. Verificar Configuración**:

En `appsettings.json`:
```json
{
  "UseRealCosmosDb": false,  // ¿Está en false para desarrollo?
  "ConnectionStrings": {
    "CosmosDb": "AccountEndpoint=..."  // ¿Connection string correcto?
  }
}
```

**2. Verificar Emulador Local** (si usas):
```powershell
# Verificar si el emulador está corriendo
Get-Process -Name "CosmosDB.Emulator" -ErrorAction SilentlyContinue

# Iniciar emulador
Start-Process "C:\Program Files\Azure Cosmos DB Emulator\CosmosDB.Emulator.exe"
```

**3. Verificar Azure Cosmos DB**:
- Ir a Azure Portal
- Verificar que la cuenta existe
- Verificar que la base de datos "PhotosMarketDb" fue creada
- Verificar keys en "Keys" section

#### Soluciones

**Usar In-Memory (Desarrollo)**:
```json
{
  "UseRealCosmosDb": false
}
```
⚠️ **Advertencia**: Los datos se pierden al reiniciar.

**Instalar Emulador**:
1. Descargar: https://aka.ms/cosmosdb-emulator
2. Instalar y ejecutar
3. Usar connection string por defecto del emulador

**Connection String Incorrecta**:
1. Azure Portal → Tu Cosmos DB
2. "Keys" → "Primary Connection String"
3. Copiar y pegar en `appsettings.json`

### Google OAuth Falla

#### Síntoma
Al hacer login con Google, error "redirect_uri_mismatch" u otro.

#### Diagnóstico

**1. Verificar Redirect URI**:

En `appsettings.json`:
```json
{
  "GoogleOAuth": {
    "RedirectUri": "http://localhost:3001/callback"  // ¿Coincide con Google Console?
  }
}
```

En Google Cloud Console:
1. Ir a APIs & Services → Credentials
2. Abrir tu OAuth 2.0 Client ID
3. Verificar "Authorized redirect URIs"
4. Debe estar **exactamente**: `http://localhost:3001/callback`

**2. Verificar Client ID y Secret**:
```json
{
  "GoogleOAuth": {
    "ClientId": "tu-clientid.apps.googleusercontent.com",  // ¿Sin espacios extra?
    "ClientSecret": "tu-client-secret"  // ¿Sin espacios o saltos de línea?
  }
}
```

**3. Verificar Estado de la App OAuth**:
- En Google Cloud Console
- Estado debe ser "Testing" o "Production"
- Si está en "Testing", agregar usuarios de prueba

#### Soluciones

**Redirect URI Mismatch**:
1. Copiar el redirect URI exacto del error
2. Agregarlo en Google Cloud Console
3. Esperar 5 minutos para que se propague

**Credenciales Inválidas**:
1. Generar nuevas credenciales en Google Console
2. Descargar JSON o copiar Client ID/Secret
3. Actualizar en `appsettings.json`

**App No Verificada**:
- Normal en desarrollo
- Los usuarios verán advertencia "App no verificada"
- Hacer clic en "Avanzado" → "Ir a PhotosMarket (no seguro)"
- En producción, solicitar verificación de Google

### Google Drive API Problemas

#### Síntoma
No se muestran álbumes o fotos, o errores al acceder a Drive.

#### Diagnóstico

**1. Verificar Service Account**:
```powershell
# Verificar que el archivo existe
Test-Path src/backend/google-drive-credentials.json
```

**2. Verificar Permisos**:
- Abrir Google Drive
- Ir a la carpeta raíz configurada
- Clic derecho → "Compartir"
- Verificar que el Service Account email tiene acceso

**3. Verificar Folder ID**:
```json
{
  "GoogleDrive": {
    "RootFolderId": "1ABC...XYZ"  // ¿Es correcto?
  }
}
```

Verificar:
- URL de la carpeta: `https://drive.google.com/drive/folders/1ABC...XYZ`
- El ID después de `/folders/` debe coincidir

#### Soluciones

**Service Account Sin Acceso**:
1. Abrir `google-drive-credentials.json`
2. Copiar el campo `client_email`
3. En Google Drive, compartir la carpeta con ese email
4. Dar permisos de "Lector" o "Editor"

**API No Habilitada**:
1. Google Cloud Console → APIs & Services → Library
2. Buscar "Google Drive API"
3. Hacer clic en "Enable"

**Folder ID Incorrecto**:
1. Navegar a la carpeta en Google Drive
2. Copiar ID de la URL
3. Actualizar en `appsettings.json`

### Errores de CORS

#### Síntoma
Frontend muestra error "CORS policy" en consola del navegador.

#### Diagnóstico

Abrir consola del navegador (F12) y buscar error similar a:
```
Access to XMLHttpRequest has been blocked by CORS policy:
No 'Access-Control-Allow-Origin' header is present...
```

#### Soluciones

**Verificar Puertos Permitidos** en `Program.cs`:
```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
            "http://localhost:3000",
            "http://localhost:3001",  // ¿Tu puerto está aquí?
            "http://localhost:3002",
            "http://localhost:5173"
        );
    });
});
```

**Si usas puerto diferente**:
1. Editar `Program.cs`
2. Agregar tu puerto a la lista. Por ejemplo:
   ```csharp
   "http://localhost:4000"
   ```
3. Recompilar y reiniciar backend

**En producción**:
```csharp
var frontendUrl = builder.Configuration["Application:FrontendUrl"];
policy.WithOrigins(frontendUrl);
```

### Emails No Se Envían

Ver sección completa en [Cliente No Recibe Emails](#cliente-no-recibe-emails).

Resumen rápido:
1. Verificar `Email.Enabled: true`
2. Si usas Resend: Verificar API Key y dominio verificado
3. Si usas SMTP: Verificar App Password (no contraseña normal)
4. Revisar logs del backend por errores
5. Verificar en Resend Dashboard si los emails se enviaron

### Despliegue en Azure Falla

#### Síntoma
`Deploy-PhotosMarket.ps1` falla o `az deployment` muestra errores.

#### Diagnóstico

**1. Verificar Login**:
```powershell
# Verificar cuenta actual
az account show

# Ver suscripciones disponibles
az account list --output table
```

**2. Verificar Permisos**:
```powershell
# Ver roles asignados
az role assignment list --assignee "tu-email@domain.com"
```

Necesitas al menos:
- `Contributor` en la suscripción o resource group
- `User Access Administrator` (para asignar roles)

**3. Revisar Mensajes de Error**:
```powershell
# Deploy con verbose para más detalles
az deployment group create \
  --resource-group rg-photosmarket-dev \
  --template-file infra/main.bicep \
  --verbose
```

#### Soluciones Comunes

**No Autenticado**:
```powershell
az login
# Seleccionar suscripción
az account set --subscription "nombre-o-id-suscripción"
```

**Resource Group No Existe**:
```powershell
az group create \
  --name rg-photosmarket-dev \
  --location eastus
```

**Nombres de Recursos Duplicados**:
- Los nombres de recursos de Azure deben ser únicos globalmente
- Modificar `environmentName` en `main.bicepparam`:
  ```bicep
  param environmentName = 'dev-tuusuario'
  ```

**Cuotas Excedidas**:
- Verificar cuotas en Azure Portal
- Solicitar aumento de cuota si es necesario
- Cambiar región (location) si una está saturada

**Errores de Bicep**:
```powershell
# Validar template antes de deploy
az deployment group validate \
  --resource-group rg-photosmarket-dev \
  --template-file infra/main.bicep \
  --parameters infra/main.bicepparam
```

### Imágenes No Se Descargan

#### Síntoma
Al hacer clic en descargar, no pasa nada o muestra error.

#### Diagnóstico

**1. Verificar Navegador**:
- Abrir consola del navegador (F12)
- Ir a pestaña "Network"
- Intentar descargar
- Ver si hay errores HTTP (401, 403, 404, 500)

**2. Verificar Backend Logs**:
```powershell
cd src/backend
dotnet run
# Intentar descargar y ver errores en consola
```

#### Soluciones

**Token Expirado**:
- Error 410 (Gone) o "Download link expired"
- Regenerar enlace de descarga (ver sección correspondiente)

**No Autorizado**:
- Error 401 (Unauthorized)
- Verificar que el token JWT es válido
- Re-autenticar si es necesario

**Archivo No Encontrado**:
- Error 404 (Not Found)
- Verificar que la foto existe en Google Drive
- Verificar que el Service Account tiene acceso

**Error del Backend**:
- Error 500 (Internal Server Error)
- Revisar logs del backend
- Verificar Google Drive API está funcionando

---

## 📞 Soporte y Ayuda

### Recursos Adicionales

- **Documentación Completa**: [README.md](../README.md)
- **Especificación Técnica**: [specification/README.md](../specification/README.md)
- **Infraestructura Azure**: [infra/README.md](../infra/README.md)
- **Scripts de Despliegue**: [scripts/README.md](../scripts/README.md)

### Repositorio y Código

- **GitHub**: [PhotosMarket Repository]
- **Issues**: Reportar bugs y solicitar features
- **Discussions**: Preguntas y respuestas de la comunidad

### Contacto

Para soporte técnico:
- 📧 Email: support@photosmarket.com (configurar)
- 💬 Discord/Slack: (configurar comunidad)

---

**Última Actualización**: Marzo 31, 2026

**Versión del Manual**: 2.0

**PhotosMarket** - Sistema profesional de venta de fotografías 📸
