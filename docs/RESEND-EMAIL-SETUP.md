# Configuración de Notificaciones por Email con Resend

Este documento explica cómo configurar el envío de notificaciones por correo electrónico usando Resend en PhotosMarket.

## 📧 ¿Qué es Resend?

Resend es un servicio moderno de email transaccional diseñado específicamente para desarrolladores. Ofrece:
- **Plan gratuito**: 3,000 emails/mes + 100 emails/día
- **API simple y moderna**: Fácil integración con .NET
- **Excelente experiencia de desarrollador**: Dashboard intuitivo
- **Sin verificación de dominio inicial**: Puedes empezar con `onboarding@resend.dev`

## 🎯 Notificaciones Implementadas

PhotosMarket envía correos automáticos en los siguientes casos:

### 1. **Orden Creada (AwaitingPayment)**
Cuando un cliente crea una orden:
- ✅ Confirmación de orden recibida
- ✅ Resumen de productos y precios
- ✅ Instrucciones de pago
- ✅ **Mensaje**: "Tu orden será procesada una vez que el pago sea realizado y verificado"

### 2. **Orden Completada (Completed)**
Cuando el administrador marca una orden como completada:
- ✅ Confirmación de orden procesada
- ✅ **SIN link de descarga** (acceso desde la aplicación)
- ✅ Agradecimiento por la compra

## 🚀 Configuración Paso a Paso

### Paso 1: Crear Cuenta en Resend

1. Visita: https://resend.com/signup
2. Crea una cuenta gratuita
3. Verifica tu email

### Paso 2: Generar API Key

1. En el dashboard de Resend, ve a **API Keys**
2. Click en **Create API Key**
3. Dale un nombre: `PhotosMarket Production` o `PhotosMarket Dev`
4. Permisos: **Sending access**
5. Copia el API Key (empieza con `re_...`)

### Paso 3: Verificar Email del Remitente (Single Sender)

**Para comenzar** (desarrollo/testing):
1. Ve a **Settings** → **Sender Verification**
2. Selecciona **Single Sender Verification**
3. Ingresa tu email (puede ser Gmail, Outlook, etc.)
4. Verifica tu email haciendo click en el link que recibes
5. ¡Listo! Ya puedes enviar emails desde esa dirección

**Para producción** (opcional - después):
1. Ve a **Settings** → **Domain**
2. Agrega tu dominio (ej: `photos-market.com`)
3. Configura los registros DNS que te proporciona Resend
4. Usa emails como `noreply@photos-market.com`

### Paso 4: Configurar el Backend

Edita `appsettings.json` en el backend:

```json
{
  "Email": {
    "Enabled": true,
    "ApiKey": "re_TU_API_KEY_AQUI",
    "SenderEmail": "tu-email-verificado@gmail.com",
    "SenderName": "Photos Market"
  }
}
```

**Parámetros:**
- `Enabled`: `true` para activar emails, `false` para desactivar
- `ApiKey`: Tu API Key de Resend (comienza con `re_`)
- `SenderEmail`: El email que verificaste en el Paso 3
- `SenderName`: Nombre que aparecerá como remitente

### Paso 5: Variables de Entorno (Producción Azure)

Para Azure Container Apps, configura las siguientes variables de entorno:

```bash
Email__Enabled=true
Email__ApiKey=re_TU_API_KEY_REAL
Email__SenderEmail=noreply@tu-dominio.com
Email__SenderName=Photos Market
```

**Opción segura (recomendada):**
Guarda el `ApiKey` en Azure Key Vault y referéncialo como secreto.

## 📝 Ejemplos de Emails

### Email de Orden Creada (AwaitingPayment)

```
Asunto: Orden Recibida - En Espera de Pago #ABC12345

¡Gracias por tu pedido!

Resumen del Pedido:
- Número de Pedido: ABC12345
- Cantidad de Fotos: 10
- Total a Pagar: CLP 10,000

Estado: En Espera de Pago
⏳ Tu orden será procesada una vez que el pago sea realizado y verificado.

Próximos Pasos:
1. Realiza la transferencia por CLP 10,000
2. Envía el comprobante indicando tu número de pedido: ABC12345
3. Recibirás un correo de confirmación cuando tu orden esté lista
```

### Email de Orden Completada

```
Asunto: Orden Completada #ABC12345 ✅

¡Tu orden ha sido completada! ✅

Tu pedido de 10 fotografías ha sido procesado exitosamente.

✅ Orden Completada
Tu pedido ha sido completado. Gracias por tu compra.

Para acceder a tus fotografías, dirígete a la sección de órdenes en tu cuenta.
```

## 🔍 Testing

### Probar en Desarrollo

1. Configura `appsettings.json` con tu API Key de prueba
2. Crea una orden desde el frontend
3. Revisa tu email para ver la confirmación
4. Marca la orden como completada desde el admin
5. Verifica el segundo email

### Dashboard de Resend

En https://resend.com/emails puedes:
- Ver todos los emails enviados
- Estado de entrega
- Logs de errores
- Métricas de apertura/clicks

## 🐛 Troubleshooting

### Error: "Email service is disabled"
**Solución**: Verifica que `Email:Enabled` esté en `true` en `appsettings.json`

### Error: "Invalid API key"
**Solución**: 
- Verifica que el API Key esté correcto
- Asegúrate de que comience con `re_`
- Regenera el API Key si es necesario

### Email no llega
**Posibles causas**:
1. Email del remitente no verificado → Ve a Settings → Sender Verification
2. API Key sin permisos de envío → Crea un nuevo API Key con "Sending access"
3. Email en spam → Revisa la carpeta de spam
4. Límite de emails alcanzado → Revisa tu dashboard de Resend

### Email enviado pero con "via resend.net"
**Esto es normal** con Single Sender Verification.
**Solución**: Para producción, configura Domain Authentication.

## 💰 Límites del Plan Gratuito

- **3,000 emails/mes**
- **100 emails/día**
- **Retención de logs**: 30 días

Para volúmenes mayores, considera upgradear al plan Pro ($20/mes por 50k emails).

## 📚 Recursos

- **Dashboard Resend**: https://resend.com/overview
- **Documentación**: https://resend.com/docs
- **Pricing**: https://resend.com/pricing
- **API Reference**: https://resend.com/docs/api-reference/introduction

## 🔐 Seguridad

### Buenas Prácticas

1. **NO** commitear el API Key en Git
2. Usar variables de entorno en producción
3. Rotar API Keys periódicamente
4. Usar Azure Key Vault para producción
5. Monitorear el dashboard para actividad sospechosa

### Azure Key Vault (Recomendado)

```csharp
// En Program.cs
var keyVaultUrl = Environment.GetEnvironmentVariable("KEY_VAULT_URL");
if (!string.IsNullOrEmpty(keyVaultUrl))
{
    var client = new SecretClient(
        new Uri(keyVaultUrl), 
        new DefaultAzureCredential()
    );
    var apiKey = await client.GetSecretAsync("ResendApiKey");
    builder.Configuration["Email:ApiKey"] = apiKey.Value.Value;
}
```

## ✅ Checklist de Configuración

- [ ] Cuenta Resend creada
- [ ] API Key generada
- [ ] Email del remitente verificado
- [ ] `appsettings.json` configurado
- [ ] Backend compilado exitosamente
- [ ] Email de prueba enviado
- [ ] Variables de entorno configuradas en Azure (producción)
- [ ] Monitoreo configurado en Resend dashboard

---

**¿Necesitas ayuda?** Revisa los logs del backend o el dashboard de Resend para diagnosticar problemas.
