# 🚀 Scripts de Inicio y Detención - PhotosMarket

Scripts de PowerShell para gestionar fácilmente los proyectos Backend y Frontend de PhotosMarket.

## 📋 Requisitos Previos

- **.NET 8 SDK** instalado
- **Node.js** (v16 o superior) y **npm** instalados
- **PowerShell 7+** (recomendado)

## 🎯 Scripts Disponibles

### 1. `Start-PhotosMarket.ps1` - Iniciar Servicios

Inicia el backend y/o frontend en procesos separados.

#### Uso Básico

```powershell
# Iniciar backend y frontend
.\Start-PhotosMarket.ps1

# Solo backend
.\Start-PhotosMarket.ps1 -BackendOnly

# Solo frontend
.\Start-PhotosMarket.ps1 -FrontendOnly
```

#### Lo que hace:
- ✅ Verifica que los servicios no estén ya corriendo
- ✅ Instala dependencias del frontend si es necesario (`npm install`)
- ✅ Inicia el backend en http://localhost:5001
- ✅ Inicia el frontend en http://localhost:3001
- ✅ Abre ventanas separadas para los logs de cada servicio

### 2. `Stop-PhotosMarket.ps1` - Detener Servicios

Detiene todos los procesos relacionados con PhotosMarket.

#### Uso Básico

```powershell
# Detener backend y frontend
.\Stop-PhotosMarket.ps1

# Solo backend
.\Stop-PhotosMarket.ps1 -BackendOnly

# Solo frontend
.\Stop-PhotosMarket.ps1 -FrontendOnly
```

#### Lo que hace:
- ✅ Busca y detiene procesos del backend (.NET)
- ✅ Busca y detiene procesos del frontend (Node/Vite)
- ✅ Cierra ventanas adicionales de PowerShell
- ✅ Libera los puertos 5001 y 3001

## 🌐 URLs de Acceso

Después de iniciar los servicios:

| Servicio | URL | Descripción |
|----------|-----|-------------|
| **Frontend** | http://localhost:3001 | Aplicación Vue.js |
| **Backend API** | http://localhost:5001 | API REST (.NET) |
| **Swagger UI** | http://localhost:5001/swagger | Documentación interactiva de API |

## 📝 Ejemplos de Uso

### Flujo de Desarrollo Normal

```powershell
# 1. Iniciar todo
.\Start-PhotosMarket.ps1

# 2. Trabajar en tu código...
# Los cambios se recargan automáticamente

# 3. Detener cuando termines
.\Stop-PhotosMarket.ps1
```

### Solo Backend (desarrollo de API)

```powershell
# Iniciar
.\Start-PhotosMarket.ps1 -BackendOnly

# Detener
.\Stop-PhotosMarket.ps1 -BackendOnly
```

### Solo Frontend (desarrollo de UI)

```powershell
# Iniciar
.\Start-PhotosMarket.ps1 -FrontendOnly

# Detener
.\Stop-PhotosMarket.ps1 -FrontendOnly
```

## 🔧 Configuración

### Backend
La configuración del backend se encuentra en:
```
src/backend/appsettings.json
```

Configuraciones importantes:
- **Email.Enabled**: `true` para activar notificaciones
- **Email.ApiKey**: Tu API Key de Resend
- **Email.SenderEmail**: Email verificado del remitente

### Frontend
El frontend se conecta automáticamente al backend en `http://localhost:5001`.

## 🐛 Troubleshooting

### Error: "Puerto ya en uso"

**Problema**: El puerto 5001 o 3001 ya está ocupado.

**Solución**:
```powershell
# Detener todos los servicios
.\Stop-PhotosMarket.ps1

# O manualmente buscar el proceso
netstat -ano | findstr :5001
taskkill /PID <PID> /F
```

### Error: "node_modules no encontrado"

**Problema**: Dependencias del frontend no instaladas.

**Solución**:
```powershell
cd src\frontend
npm install
cd ..\..
.\Start-PhotosMarket.ps1
```

### Error: "Resend API Key inválido"

**Problema**: La configuración de email no es correcta.

**Solución**:
1. Verifica que `Email.Enabled` esté en `true` en `appsettings.json`
2. Verifica que `Email.ApiKey` contenga tu API Key de Resend (comienza con `re_`)
3. Consulta `docs/RESEND-EMAIL-SETUP.md` para más detalles

### Backend no inicia

**Solución**:
```powershell
# Compilar manualmente para ver errores
cd src\backend
dotnet build
dotnet run
```

### Frontend no inicia

**Solución**:
```powershell
# Ejecutar manualmente para ver errores
cd src\frontend
npm install
npm run dev
```

## 📦 Estructura de Archivos

```
photos-market/
├── Start-PhotosMarket.ps1        # Script de inicio
├── Stop-PhotosMarket.ps1          # Script de detención
├── SCRIPTS-GUIDE.md               # Esta guía
├── src/
│   ├── backend/
│   │   ├── appsettings.json       # Configuración del backend
│   │   └── PhotosMarket.API.csproj
│   └── frontend/
│       ├── package.json
│       └── vite.config.js
└── docs/
    └── RESEND-EMAIL-SETUP.md      # Configuración de emails
```

## 💡 Tips y Mejores Prácticas

1. **Siempre ejecuta desde la raíz**: Los scripts deben ejecutarse desde `c:\repos\microsoft\photos-market`

2. **Logs en tiempo real**: Los scripts abren ventanas separadas para los logs de cada servicio, así puedes ver errores en tiempo real.

3. **Hot Reload**: 
   - El backend recarga automáticamente con cambios en archivos `.cs`
   - El frontend recarga automáticamente con cambios en archivos `.vue`, `.js`, `.css`

4. **Múltiples instancias**: Los scripts detectan si ya hay servicios corriendo y te avisan.

5. **Limpieza automática**: `Stop-PhotosMarket.ps1` limpia todos los procesos relacionados.

## 🔐 Configuración de Emails

Para probar las notificaciones por email:

1. **Configura Resend** (gratis):
   - Crea cuenta en https://resend.com
   - Genera API Key
   - Verifica tu email de remitente

2. **Actualiza `appsettings.json`**:
```json
{
  "Email": {
    "Enabled": true,
    "ApiKey": "re_TU_API_KEY",
    "SenderEmail": "tu-email@gmail.com",
    "SenderName": "Photos Market"
  }
}
```

3. **Prueba**:
   - Crea una orden
   - Verifica que llegue el email de "En Espera de Pago"
   - Marca la orden como completada
   - Verifica que llegue el email de "Orden Completada"

Consulta `docs/RESEND-EMAIL-SETUP.md` para más detalles.

## 📚 Recursos

- [Documentación Backend](src/backend/README.md)
- [Documentación Frontend](src/frontend/README.md)
- [Configuración de Resend](docs/RESEND-EMAIL-SETUP.md)
- [Guía de Despliegue](docs/DEPLOY-INFRA-WORKFLOW.md)

## ❓ Ayuda

Si encuentras problemas:

1. Revisa los logs en las ventanas de terminal
2. Ejecuta `.\Stop-PhotosMarket.ps1` y vuelve a intentar
3. Verifica la configuración en `appsettings.json`
4. Consulta la documentación en `/docs`

---

**¿Listo para empezar?**

```powershell
.\Start-PhotosMarket.ps1
```

Luego abre tu navegador en http://localhost:3001 🚀
