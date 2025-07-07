# Rent2Go Backend - Configuración de Variables de Entorno

## 🔧 Configuración para Desarrollo Local

### 1. Crear archivo de variables de entorno
Crea un archivo `.env.local` en la raíz del proyecto con las siguientes variables:

```bash
# Base de datos
ConnectionStrings__DefaultConnection=Host=tu-host;Database=tu-db;Username=tu-usuario;Password=tu-password;SSL Mode=Require;Trust Server Certificate=true

# JWT Token Key (debe ser una clave segura de al menos 64 bytes)
TokenKey=tu_clave_jwt_muy_segura_aqui

# SendGrid (para envío de emails)
SendGridSettings__ApiKey=SG.tu_api_key_de_sendgrid_aqui

# Autenticación con Google (opcional)
Authentication__Google__ClientId=tu_client_id_de_google
Authentication__Google__ClientSecret=tu_client_secret_de_google

# Autenticación con Facebook (opcional)
Authentication__Facebook__AppId=tu_app_id_de_facebook
Authentication__Facebook__AppSecret=tu_app_secret_de_facebook
```

### 2. Ejecutar en desarrollo
```bash
dotnet run
```

El archivo `.env.local` se cargará automáticamente en modo desarrollo.

## 🚀 Configuración para Deploy en Render

### Variables de entorno a configurar en Render:

1. **ConnectionStrings__DefaultConnection**
   - Tu cadena de conexión de PostgreSQL

2. **TokenKey**
   - Clave secreta para JWT (mínimo 64 caracteres)

3. **SendGridSettings__ApiKey**
   - Tu API Key de SendGrid

4. **Authentication__Google__ClientId** (opcional)
   - Client ID de Google OAuth

5. **Authentication__Google__ClientSecret** (opcional)
   - Client Secret de Google OAuth

6. **Authentication__Facebook__AppId** (opcional)
   - App ID de Facebook

7. **Authentication__Facebook__AppSecret** (opcional)
   - App Secret de Facebook

### Configurar en Render:
1. Ve a tu servicio en Render
2. Selecciona "Environment"
3. Agrega cada variable con su valor correspondiente
4. Guarda y redeploya

## 📝 Notas Importantes

- ❌ **NUNCA** subas archivos `.env.local` o `.env` al repositorio
- ✅ Los archivos de configuración (`appsettings.json`) ahora están limpios sin claves sensibles
- ✅ El proyecto carga automáticamente variables de entorno según el entorno
- ✅ En desarrollo usa `.env.local`, en producción usa variables de entorno del sistema

## 🔐 Seguridad

- Todas las claves sensibles están ahora fuera del código fuente
- GitHub no bloqueará más los pushes por claves expuestas
- El deploy en Render funcionará correctamente con variables de entorno

## 🛠️ Comandos útiles

```bash
# Verificar que la aplicación compile
dotnet build

# Ejecutar en desarrollo
dotnet run

# Publicar para producción
dotnet publish -c Release
```
