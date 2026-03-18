# ✅ Checklist de Configuración de GitHub Actions

## Fecha de inicio: _____________

---

## 🔧 Preparación Inicial

- [ ] Azure CLI instalado y funcionando
  ```bash
  az --version
  ```

- [ ] Sesión de Azure activa
  ```bash
  az login
  az account show
  ```

- [ ] Git configurado localmente
  ```bash
  git --version
  git config --global user.name
  git config --global user.email
  ```

- [ ] Repositorio de GitHub creado
  - Nombre del repo: ______________________________
  - URL: https://github.com/_______________________

---

## 📝 PASO 1: Crear Service Principal

- [ ] Ejecutar script de setup:
  ```powershell
  .\scripts\setup-github-actions.ps1
  ```

- [ ] Service Principal creado exitosamente
  - Nombre: ________________________________________
  - Client ID: _____________________________________

- [ ] JSON de credenciales copiado al portapapeles
  - ✅ Guardado temporalmente en archivo seguro

- [ ] Permisos de ACR asignados
  ```bash
  az role assignment list --assignee CLIENT_ID
  ```

---

## 🔐 PASO 2: Configurar GitHub Secrets

Ve a: `https://github.com/TU_USUARIO/TU_REPO/settings/secrets/actions`

### Secret 1: AZURE_CREDENTIALS
- [ ] Secret creado en GitHub
  - Nombre exacto: `AZURE_CREDENTIALS`
  - Valor: JSON completo del Service Principal
  - Verificado: ✅

### Secret 2: GOOGLE_OAUTH_CLIENT_ID
- [ ] Obtenido de Google Cloud Console
  - URL: https://console.cloud.google.com/apis/credentials
  - Proyecto: ______________________________________
  - Client ID: _____________________________________

- [ ] Secret creado en GitHub
  - Nombre exacto: `GOOGLE_OAUTH_CLIENT_ID`
  - Verificado: ✅

### Secret 3: GOOGLE_OAUTH_CLIENT_SECRET
- [ ] Obtenido de Google Cloud Console
  - Client Secret: _________________________________

- [ ] Secret creado en GitHub
  - Nombre exacto: `GOOGLE_OAUTH_CLIENT_SECRET`
  - Verificado: ✅

### Secret 4: JWT_SECRET_KEY
- [ ] Clave generada (mínimo 32 caracteres)
  - Generado por script: ✅
  - Clave: (NO ESCRIBIR AQUÍ - mantener segura)

- [ ] Secret creado en GitHub
  - Nombre exacto: `JWT_SECRET_KEY`
  - Verificado: ✅

### Secret 5: GOOGLE_DRIVE_ROOT_FOLDER_ID
- [ ] Carpeta de Google Drive creada/identificada
  - Nombre de carpeta: _____________________________
  - URL: https://drive.google.com/drive/folders/___
  - Folder ID: _____________________________________

- [ ] Secret creado en GitHub
  - Nombre exacto: `GOOGLE_DRIVE_ROOT_FOLDER_ID`
  - Verificado: ✅

---

## 📤 PASO 3: Commit y Push de Workflows

- [ ] Revisar workflows creados:
  ```bash
  ls .github/workflows/
  ```
  Archivos esperados:
  - [ ] `ci.yml`
  - [ ] `deploy-backend.yml`
  - [ ] `deploy-frontend.yml`
  - [ ] `deploy-infra.yml`
  - [ ] `full-deploy.yml`

- [ ] Agregar archivos al staging:
  ```bash
  git add .github/
  git add scripts/setup-github-actions.ps1
  git add .gitignore
  ```

- [ ] Commit de cambios:
  ```bash
  git commit -m "ci: add GitHub Actions workflows for CI/CD"
  ```

- [ ] Push a main:
  ```bash
  git push origin main
  ```

---

## 🚀 PASO 4: Verificar Primer Deployment

- [ ] Workflows visibles en GitHub
  - URL: https://github.com/TU_USUARIO/TU_REPO/actions

- [ ] Workflow "Full Deploy to Azure" ejecutándose
  - Inicio: ________________________________________
  - Estado: En progreso / Completado / Fallido

- [ ] Job "build-and-push-images" completado
  - Backend image pushed: ✅
  - Frontend image pushed: ✅

- [ ] Job "deploy-backend" completado
  - Revision Name: _________________________________

- [ ] Job "deploy-frontend" completado
  - Revision Name: _________________________________

- [ ] Job "health-check" completado
  - Backend URL: ___________________________________
  - Frontend URL: __________________________________

---

## ✅ PASO 5: Verificación Final

### Backend
- [ ] Container App corriendo:
  ```bash
  az containerapp show \
    --name photosmarket-backend-dev \
    --resource-group rg-photosmarket-dev \
    --query "properties.runningStatus"
  ```

- [ ] URL accesible:
  - [ ] https://[backend-url]/swagger carga correctamente
  - [ ] API responde a requests

### Frontend
- [ ] Container App corriendo:
  ```bash
  az containerapp show \
    --name photosmarket-frontend-dev \
    --resource-group rg-photosmarket-dev \
    --query "properties.runningStatus"
  ```

- [ ] URL accesible:
  - [ ] https://[frontend-url] carga correctamente
  - [ ] Página de login visible
  - [ ] Recursos estáticos cargando

### Integración
- [ ] OAuth de Google configurado:
  - [ ] URLs autorizadas agregadas en Google Console
  - [ ] Login con Google funciona

- [ ] Conexión Backend ↔ Frontend:
  - [ ] API calls exitosos
  - [ ] CORS configurado correctamente

---

## 🔄 PASO 6: Probar CI/CD

### Prueba 1: Cambio en Backend
- [ ] Crear rama de feature:
  ```bash
  git checkout -b test/backend-change
  ```

- [ ] Hacer un cambio menor en backend
  - Archivo modificado: ____________________________

- [ ] Push y crear PR:
  ```bash
  git push origin test/backend-change
  ```

- [ ] CI workflow ejecutado en PR: ✅
- [ ] Merge a main
- [ ] Deploy workflow ejecutado: ✅
- [ ] Backend actualizado en Azure: ✅

### Prueba 2: Cambio en Frontend
- [ ] Crear rama de feature:
  ```bash
  git checkout -b test/frontend-change
  ```

- [ ] Hacer un cambio menor en frontend
  - Archivo modificado: ____________________________

- [ ] Push y crear PR
- [ ] CI workflow ejecutado en PR: ✅
- [ ] Merge a main
- [ ] Deploy workflow ejecutado: ✅
- [ ] Frontend actualizado en Azure: ✅

---

## 🧹 PASO 7: Limpieza

- [ ] Eliminar archivo de secrets temporales:
  ```powershell
  Remove-Item "scripts\github-secrets-temp.txt" -ErrorAction SilentlyContinue
  ```

- [ ] Verificar que no hay credenciales en el repo:
  ```bash
  git status
  git log --all --full-history --source -- *secrets* *credentials*
  ```

- [ ] Documentar URLs de producción:
  - Backend: _______________________________________
  - Frontend: ______________________________________

---

## 📊 Monitoreo Continuo

### Configurar Badges (Opcional)
- [ ] Agregar badges al README.md principal
  ```markdown
  ![Backend](https://github.com/USER/REPO/actions/workflows/deploy-backend.yml/badge.svg)
  ![Frontend](https://github.com/USER/REPO/actions/workflows/deploy-frontend.yml/badge.svg)
  ```

### Configurar Notificaciones (Opcional)
- [ ] Slack webhook configurado
- [ ] Email notifications configuradas
- [ ] Teams integration configurada

---

## 🎯 KPIs de Éxito

- [ ] Deployment time < 10 minutos
- [ ] Success rate > 95%
- [ ] Zero manual deployments
- [ ] Automated rollback configurado
- [ ] Logs centralizados accesibles

---

## 📝 Notas Adicionales

### Problemas Encontrados
- Issue 1: __________________________________________
  Solución: __________________________________________

- Issue 2: __________________________________________
  Solución: __________________________________________

### Mejoras Futuras
- [ ] Agregar environment de staging
- [ ] Implementar blue-green deployment
- [ ] Agregar smoke tests post-deployment
- [ ] Configurar auto-scaling rules
- [ ] Implementar feature flags

---

## ✅ CONFIRMACIÓN FINAL

- [ ] ✅ Todos los workflows funcionando
- [ ] ✅ Aplicación desplegada y accesible
- [ ] ✅ CI/CD completamente automatizado
- [ ] ✅ Documentación actualizada
- [ ] ✅ Equipo notificado

**Fecha de completación: _____________**

**Configurado por: _____________**

**Revisado por: _____________**

---

## 🎉 ¡FELICITACIONES!

Tu pipeline de CI/CD está completamente configurado y funcionando.

Cada push a `main` ahora desplegará automáticamente tu aplicación a Azure.

### Próximos pasos sugeridos:
1. Configurar un environment de staging
2. Agregar más tests automatizados
3. Implementar monitoring y alerting
4. Documentar el proceso de rollback
5. Capacitar al equipo en el nuevo workflow

**¡Happy Deploying! 🚀**
