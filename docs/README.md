# Documentación de despliegues y troubleshooting

## Workflow recomendado cuando falla la conexión con Google después de un cambio de infraestructura

Este documento resume el procedimiento que debe seguirse cuando el login con Google deja de funcionar luego de desplegar cambios de infraestructura.

### Síntomas comunes

- Error de red al intentar iniciar sesión con Google
- Mensaje tipo `Network Error` o `ERR_NAME_NOT_RESOLVED`
- El botón de Google no responde o el flujo de OAuth se rompe

### Causa más frecuente

Después de un cambio de infraestructura, los valores de URL usados por frontend y backend pueden quedar desactualizados o incompletos.

Cuando eso pasa, normalmente ocurre que:

- el frontend apunta a una URL de backend incorrecta
- el backend usa una URL de frontend incorrecta
- Google OAuth recibe un redirect URI inválido o desactualizado

## Procedimiento obligatorio

Cuando haya un cambio de infraestructura, siempre debe hacerse este orden:

1. Desplegar el frontend
2. Desplegar el backend
3. Verificar que ambas variables queden con el FQDN real y no con un valor incompleto como `https://`

### Orden correcto

```powershell
# 1) Desplegar frontend primero
./scripts/Deploy-PhotosMarket.ps1 -Component Frontend

# 2) Desplegar backend después
./scripts/Deploy-PhotosMarket.ps1 -Component Backend
```

> Si se hace solo un despliegue parcial o se cambia la infraestructura sin actualizar ambos servicios, el problema de Google Auth suele volver a aparecer.

## Validaciones que deben hacerse después del despliegue

### 1. Verificar la URL del frontend

```powershell
az containerapp show \
  --name <frontend-container-app> \
  --resource-group <resource-group> \
  --query "properties.template.containers[0].env[?name=='VITE_API_URL'].value" \
  --output tsv
```

El valor esperado debe ser algo como:

```text
https://<backend-fqdn>
```

### 2. Verificar la URL del backend

```powershell
az containerapp show \
  --name <backend-container-app> \
  --resource-group <resource-group> \
  --query "properties.template.containers[0].env[?name=='FRONTEND_URL'].value" \
  --output tsv
```

El valor esperado debe ser algo como:

```text
https://<frontend-fqdn>
```

### 3. Verificar el redirect URI en Google Cloud Console

El redirect URI autorizado debe coincidir exactamente con el frontend desplegado:

```text
https://<frontend-fqdn>/callback
```

## Qué evitar

No dejar estos valores en un estado inválido:

```text
https://
```

```text
"
```

```text
vacío
```

## Resumen rápido

Si el login con Google falla después de un cambio de infraestructura:

- despliega frontend
- despliega backend
- verifica que las URLs queden correctas
- verifica el redirect URI en Google Cloud Console
- prueba nuevamente el login

## Referencias relacionadas

- [DEPLOY-INFRA-WORKFLOW.md](DEPLOY-INFRA-WORKFLOW.md)
- [INFRA-WORKFLOW-IMPROVEMENTS.md](INFRA-WORKFLOW-IMPROVEMENTS.md)
