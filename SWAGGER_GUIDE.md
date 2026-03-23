# Guía de Documentación Swagger - AuthService API

## Descripción General

La API de autenticación de KinalSports proporciona un conjunto completo de endpoints para gestionar autenticación de usuarios, verificación de email, recuperación de contraseña y gestión de roles.

## Acceso a Swagger

Durante el desarrollo, la documentación interactiva de Swagger está disponible en:

```
http://localhost:5000/
```

O específicamente en:

```
http://localhost:5000/swagger/ui/index.html
```

## Autenticación Bearer Token

La mayoría de los endpoints requieren autenticación mediante JWT Bearer Token.

### Cómo obtener un token:

1. Registra un nuevo usuario usando `POST /api/v1/auth/register`
2. Verifica tu email usando el token recibido en `POST /api/v1/auth/verify-email`
3. Inicia sesión usando `POST /api/v1/auth/login`
4. Copia el token JWT de la respuesta

### Cómo usar el token en Swagger:

1. Haz clic en el botón "Authorize" en la esquina superior derecha
2. Selecciona o escribe "Bearer" como esquema
3. Pega el token JWT en el campo de entrada (solo el token, sin "Bearer ")
4. Haz clic en "Authorize"

Alternativamente, puedes incluir el header manualmente:
```
Authorization: Bearer {tu_token_jwt}
```

## Flujo de Registro E Autenticación

### 1. Registro de un nuevo usuario

**Endpoint:** `POST /api/v1/auth/register`

- Requiere multipart/form-data
- El campo `profilePicture` es opcional
- Se envía automáticamente un email de verificación

**Ejemplo de respuesta:**
```json
{
  "success": true,
  "user": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "John",
    "surname": "Doe",
    "username": "john_doe",
    "email": "john@example.com",
    "phone": "12345678",
    "profilePicture": "https://...",
    "role": "USER",
    "status": true,
    "isEmailVerified": false,
    "createdAt": "2024-03-01T10:30:00Z",
    "updatedAt": "2024-03-01T10:30:00Z"
  },
  "message": "Usuario registrado exitosamente. Se ha enviado un email de verificación.",
  "emailVerificationRequired": true
}
```

### 2. Verificación de Email

**Endpoint:** `POST /api/v1/auth/verify-email`

- Requiere el token recibido en el email
- Una vez verificado, el usuario puede iniciar sesión normalmente

### 3. Login

**Endpoint:** `POST /api/v1/auth/login`

- Proporciona email/usuario y contraseña
- Retorna un JWT token válido por 24 horas (configurable)

**Ejemplo de respuesta:**
```json
{
  "success": true,
  "message": "Autenticación exitosa",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "userDetails": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "username": "john_doe",
    "profilePicture": "https://...",
    "role": "USER"
  },
  "expiresAt": "2024-03-02T10:30:00Z"
}
```

## Endpoints Principales

### Autenticación (Sin autenticación requerida)

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| POST | `/api/v1/auth/register` | Registra un nuevo usuario |
| POST | `/api/v1/auth/login` | Autentica un usuario |
| POST | `/api/v1/auth/verify-email` | Verifica el email del usuario |
| POST | `/api/v1/auth/resend-verification` | Reenvía email de verificación |
| POST | `/api/v1/auth/forgot-password` | Inicia recuperación de contraseña |
| POST | `/api/v1/auth/reset-password` | Restablece la contraseña |

### Perfil del Usuario (Requiere autenticación)

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/v1/auth/profile` | Obtiene perfil del usuario autenticado |

### Gestión de Usuarios (Requiere autenticación)

| Método | Endpoint | Descripción | Permisos |
|--------|----------|-------------|----------|
| GET | `/api/v1/users/{userId}/roles` | Obtiene roles de un usuario | Cualquier usuario |
| PUT | `/api/v1/users/{userId}/role` | Actualiza el rol de un usuario | Admin |
| GET | `/api/v1/users/by-role/{roleName}` | Obtiene usuarios por rol | Admin |

### Health Check

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/v1/health` | Verifica el estado del servicio |
| GET | `/health` | Health check alternativo |

## Códigos de Respuesta HTTP

- **200 OK**: La solicitud fue exitosa
- **201 Created**: Recurso creado exitosamente
- **400 Bad Request**: Datos inválidos o incompletos
- **401 Unauthorized**: No autenticado o token inválido
- **403 Forbidden**: Autenticado pero sin permisos suficientes
- **404 Not Found**: Recurso no encontrado
- **409 Conflict**: Conflict (e.g., usuario ya existe)
- **429 Too Many Requests**: Límite de tasa excedido
- **503 Service Unavailable**: Error al enviar email u otro servicio externo

## Rate Limiting

La API implementa límites de tasa para proteger contra abuso:

- **AuthPolicy**: 5 solicitudes por minuto para endpoints de autenticación
- **ApiPolicy**: 30 solicitudes por minuto para otros endpoints
- **DefaultPolicy**: Aplica a todos los demás endpoints

Si excedes el límite, recibirás un error 429.

## Modelos de Datos

### Roles Disponibles

- `USER`: Usuario estándar
- `MODERATOR`: Usuario con permisos de moderación
- `PLATFORM_ADMIN`: Administrador de plataforma

### Campos Validados

**Contraseña:**
- Mínimo 8 caracteres
- Se recomienda incluir mayúsculas, minúsculas, números y caracteres especiales

**Email:**
- Debe ser un email válido
- Debe ser único en el sistema

**Usuario:**
- Debe ser único en el sistema
- Sin espacios

**Teléfono:**
- Exactamente 8 caracteres

**Nombre/Apellido:**
- Máximo 25 caracteres cada uno

## Ejemplos de Uso

### Con cURL

**Registro:**
```bash
curl -X POST "http://localhost:5000/api/v1/auth/register" \
  -H "Content-Type: multipart/form-data" \
  -F "name=John" \
  -F "surname=Doe" \
  -F "username=john_doe" \
  -F "email=john@example.com" \
  -F "password=MySecurePass123" \
  -F "phone=12345678"
```

**Login:**
```bash
curl -X POST "http://localhost:5000/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "emailOrUsername": "john@example.com",
    "password": "MySecurePass123"
  }'
```

**Obtener Perfil (con token):**
```bash
curl -X GET "http://localhost:5000/api/v1/auth/profile" \
  -H "Authorization: Bearer {tu_token_aqui}"
```

### Con Postman

1. Importa la colección `AuthServiceRestaurante.postman_collection.json`
2. Configura la variable `base_url` con tu URL de servidor
3. Hay una variable `token` que se actualiza automáticamente después del login

## Anotaciones en Swagger

Todos los endpoints incluyen:

- **Descripción:** Explicación clara de qué hace el endpoint
- **Ejemplos:** Ejemplos de solicitudes y respuestas
- **Códigos de respuesta:** Documentación de qué esperar en diferentes casos
- **Parámetros:** Descripción de cada parámetro y su validación

## Desarrollo y Testing

- La documentación Swagger se genera automáticamente a partir de los comentarios XML del código
- Los comentarios XML aparecen en Swagger como descripciones y ejemplos
- Los tipos de datos se infieren automáticamente desde las clases DTO

## Notas Importantes

1. **Seguridad de Email:** El endpoint `forgot-password` siempre retorna éxito por razones de seguridad, incluso si el email no existe
2. **Verificación de Email:** El email debe verificarse antes de poder usar ciertos endpoints
3. **Token JWT:** Los tokens expiran después de 24 horas
4. **Contraseñas:** Nunca son devueltas en las respuestas
5. **Rate Limiting:** Se aplica por IP y endpoint

## Soporte

Para más información o problemas, contacta al equipo de desarrollo de KinalSports.

---

**Última actualización:** 3 de Marzo de 2024
**Versión API:** 1.0.0
