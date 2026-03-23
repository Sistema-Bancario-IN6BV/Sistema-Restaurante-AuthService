# API de Autenticación - KinalSports AuthService

## Descripción General

AuthService es el servicio centralizado de autenticación y gestión de usuarios de la plataforma KinalSports. Proporciona funcionalidades completas de registro, autenticación, verificación de email y gestión de roles de usuarios.

**Versión:** 1.0.0  
**Estado:** Producción  
**Último actualizado:** 3 de Marzo de 2024

---

## 📚 Documentación

### Swagger UI Interactivo

Durante el desarrollo, accede a la documentación interactiva de Swagger en:

```
http://localhost:5000/
```

O directamente en:

```
http://localhost:5000/swagger/ui
```

La documentación Swagger incluye:
- ✅ Todos los endpoints disponibles
- ✅ Modelos de datos y esquemas
- ✅ Códigos de respuesta HTTP
- ✅ Ejemplos de solicitudes y respuestas
- ✅ Parámetros requeridos y opcionales
- ✅ Autenticación Bearer Token integrada

### Documentación Offline

Se incluyen dos documentos HTML estáticos para consulta offline:

1. **[SWAGGER_GUIDE.md](SWAGGER_GUIDE.md)** - Guía detallada en Markdown
2. **[SWAGGER_DOCUMENTATION.html](SWAGGER_DOCUMENTATION.html)** - Documentación completa en HTML

---

## 🚀 Inicio Rápido

### Requisitos
- .NET 8.0 o superior
- PostgreSQL (configurado en `appsettings.json`)
- Visual Studio 2022 o VS Code

### Configuración

1. **Instala dependencias:**
```bash
dotnet restore
```

2. **Configura la base de datos** en `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=5432;Database=authservice_db;User Id=postgres;Password=your_password;"
  }
}
```

3. **Aplica las migraciones:**
```bash
dotnet ef database update -p src/AuthService_GR.Persistence -s src/AuthService_GR.Api
```

4. **Inicia la aplicación:**
```bash
dotnet run --project src/AuthService_GR.Api
```

La API estará disponible en `https://localhost:5000`

---

## 🔐 Autenticación

La mayoría de los endpoints protegidos requieren un JWT Bearer Token.

### Obtener un Token

1. **Registrar usuario:**
```bash
POST /api/v1/auth/register
```

2. **Verificar email:**
```bash
POST /api/v1/auth/verify-email
```

3. **Iniciar sesión:**
```bash
POST /api/v1/auth/login
```

4. El token será incluido en la respuesta

### Usar el Token

En Swagger:
1. Haz clic en "Authorize" (parte superior derecha)
2. Selecciona "Bearer"
3. Pega tu token JWT
4. Haz clic en "Authorize"

O incluye en el header:
```
Authorization: Bearer {tu_token_jwt}
```

---

## 📡 Endpoints Principales

### Autenticación (Sin autenticación requerida)

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| POST | `/api/v1/auth/register` | Registra un nuevo usuario |
| POST | `/api/v1/auth/login` | Autentica un usuario |
| POST | `/api/v1/auth/verify-email` | Verifica el email |
| POST | `/api/v1/auth/resend-verification` | Reenvía email de verificación |
| POST | `/api/v1/auth/forgot-password` | Inicia recuperación de contraseña |
| POST | `/api/v1/auth/reset-password` | Restablece la contraseña |

### Perfil (Requiere autenticación)

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/v1/auth/profile` | Obtiene perfil del usuario autenticado |

### Gestión de Usuarios (Requiere autenticación)

| Método | Endpoint | Descripción | Permisos |
|--------|----------|-------------|----------|
| GET | `/api/v1/users/{userId}/roles` | Obtiene roles de un usuario | Cualquiera |
| PUT | `/api/v1/users/{userId}/role` | Actualiza el rol de un usuario | Admin |
| GET | `/api/v1/users/by-role/{roleName}` | Obtiene usuarios por rol | Admin |

### Health Check

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/v1/health` | Verifica el estado del servicio |

---

## 🔑 Roles de Usuario

| Rol | Descripción | Permisos |
|-----|-------------|----------|
| USER | Usuario estándar | Acceso básico |
| MODERATOR | Moderador | Moderación de contenido |
| PLATFORM_ADMIN | Administrador | Acceso total |

---

## 📊 Códigos de Respuesta HTTP

| Código | Estado | Descripción |
|--------|--------|-------------|
| 200 | OK | Solicitud exitosa |
| 201 | Created | Recurso creado (registro) |
| 400 | Bad Request | Datos inválidos |
| 401 | Unauthorized | No autenticado/token inválido |
| 403 | Forbidden | Sin permisos suficientes |
| 404 | Not Found | Recurso no encontrado |
| 429 | Too Many Requests | Límite de tasa excedido |
| 503 | Service Unavailable | Error en servicio externo |

---

## 🛡️ Rate Limiting

Se aplican límites de tasa por IP:

- **AuthPolicy (endpoints de auth):** 5 solicitudes/minuto
- **ApiPolicy (otros endpoints):** 30 solicitudes/minuto

Respuesta cuando se excede el límite: `429 Too Many Requests`

---

## 📝 Validaciones

| Campo | Validación |
|-------|-----------|
| **Contraseña** | Mínimo 8 caracteres |
| **Email** | Debe ser válido y único |
| **Usuario** | Único, sin espacios |
| **Teléfono** | Exactamente 8 caracteres |
| **Nombre/Apellido** | Máximo 25 caracteres |

---

## 🔧 Estructura del Proyecto

```
src/
├── AuthService_GR.Api/              # Capa de presentación
│   ├── Controllers/                 # Endpoints API
│   ├── Extensions/                  # Configuraciones
│   ├── Middlewares/                 # Middleware personalizado
│   └── Models/                      # Modelos de respuesta
├── AuthService_GR.Application/      # Lógica de negocio
│   ├── DTOs/                        # Objetos de transferencia
│   ├── Interfaces/                  # Contratos de servicios
│   ├── Services/                    # Implementación de servicios
│   ├── Validators/                  # Validadores
│   └── Exceptions/                  # Excepciones personalizadas
└── AuthService_GR.Persistence/      # Acceso a datos
    ├── Data/                        # DbContext
    ├── Repositories/                # Acceso a repositorios
    └── Migrations/                  # Migraciones EF Core
```

---

## 🔍 Características de Seguridad

- ✅ JWT Bearer Token (24 horas)
- ✅ Contraseñas hash con salt (BCrypt)
- ✅ Verificación de email obligatoria
- ✅ Recuperación segura de contraseña
- ✅ Headers de seguridad (HSTS, CSP, etc.)
- ✅ Rate limiting por IP
- ✅ Protección contra CSRF
- ✅ Logs de auditoría (Serilog)

---

## 📦 Dependencias Principales

- **Swashbuckle.AspNetCore** - Documentación Swagger
- **Microsoft.AspNetCore.Authentication.JwtBearer** - Autenticación JWT
- **Entity Framework Core** - ORM
- **Serilog** - Logging
- **NetEscapades.AspNetCore.SecurityHeaders** - Headers de seguridad

---

## 🧪 Testing

### Importar en Postman

Se incluye una colección Postman: `AuthServiceRestaurante.postman_collection.json`

1. Abre Postman
2. Importa la colección
3. Configura las variables de entorno:
   - `base_url`: http://localhost:5000
   - `token`: Se actualizará automáticamente después del login

### Ejemplos con cURL

**Registro:**
```bash
curl -X POST "http://localhost:5000/api/v1/auth/register" \
  -H "Content-Type: multipart/form-data" \
  -F "name=John" \
  -F "surname=Doe" \
  -F "username=john_doe" \
  -F "email=john@example.com" \
  -F "password=SecurePass123" \
  -F "phone=12345678"
```

**Login:**
```bash
curl -X POST "http://localhost:5000/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "emailOrUsername": "john@example.com",
    "password": "SecurePass123"
  }'
```

**Obtener Perfil:**
```bash
curl -X GET "http://localhost:5000/api/v1/auth/profile" \
  -H "Authorization: Bearer {token}"
```

---

## 🗄️ Base de Datos

### Tablas Principales

- `users` - Información de usuarios
- `user_emails` - Emails verificados
- `user_password_resets` - Tokens de recuperación
- `user_profiles` - Perfiles adicionales
- `user_roles` - Relación usuario-rol
- `roles` - Definición de roles

### Migraciones

```bash
# Ver migraciones
dotnet ef migrations list -p src/AuthService_GR.Persistence

# Crear nueva migración
dotnet ef migrations add YourMigrationName -p src/AuthService_GR.Persistence -s src/AuthService_GR.Api

# Aplicar cambios
dotnet ef database update -p src/AuthService_GR.Persistence -s src/AuthService_GR.Api
```

---

## 🐳 Docker

Ejecutar con Docker Compose:

```bash
docker-compose up -d
```

El archivo `docker-compose.yml` incluye:
- Servicio de API
- Base de datos PostgreSQL

---

## 📚 Documentación Adicional

- **[SWAGGER_GUIDE.md](SWAGGER_GUIDE.md)** - Guía detallada de Swagger
- **[SWAGGER_DOCUMENTATION.html](SWAGGER_DOCUMENTATION.html)** - Documentación HTML interactiva
- **Colección Postman** - `AuthServiceRestaurante.postman_collection.json`

---

## 🤝 Contribución

Para contribuir al proyecto:

1. Crea una rama feature: `git checkout -b feature/mi-feature`
2. Realiza tus cambios
3. Crea un commit: `git commit -am 'Agrego mi feature'`
4. Push a la rama: `git push origin feature/mi-feature`
5. Crea un Pull Request

---

## 📄 Licencia

Este proyecto está bajo la licencia MIT. Ver [LICENSE](LICENSE) para más detalles.

---

## 📞 Soporte

Para problemas, preguntas o sugerencias:
- Email: support@kinalsports.com
- Sitio web: https://kinalsports.com

---

## 🎯 Roadmap Futuro

- [ ] OAuth2 / OpenID Connect
- [ ] Autenticación Multi-Factor (MFA)
- [ ] Social Login (Google, Facebook)
- [ ] Dashboard de administración
- [ ] Auditoría completa de accesos
- [ ] Integración con servicios de terceros

---

**Versión:** 1.0.0  
**Última actualización:** 3 de Marzo de 2024  
**Autor:** KinalSports Development Team
