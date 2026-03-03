# Configuración de Swagger - AuthService API

## Descripción

Este documento describe la configuración de Swagger/OpenAPI implementada en AuthService.

---

## Información General

- **Título:** AuthService API
- **Versión:** 1.0.0
- **Descripción:** API de autenticación y gestión de usuarios para KinalSports
- **Contacto:** KinalSports Development Team (support@kinalsports.com)
- **Licencia:** MIT

---

## Esquema de Autenticación

### Bearer Token (JWT)

```
Type: HTTP
Scheme: bearer
Bearer Format: JWT
Description: Ingresa un JWT Token válido para acceder a endpoints protegidos.
             Formato: Bearer {token}
```

**Cómo obtener un token:**
1. Registra un usuario: `POST /api/v1/auth/register`
2. Verifica el email: `POST /api/v1/auth/verify-email`
3. Inicia sesión: `POST /api/v1/auth/login`
4. Copia el token de la respuesta

---

## Documentación XML Comentarios

Se utilizan comentarios XML en el código fuente para generar automáticamente la documentación de Swagger.

### Estructura de Comentarios

```csharp
/// <summary>
/// Descripción breve del método/propiedad
/// </summary>
/// <remarks>
/// Descripción detallada, notas adicionales, advertencias
/// </remarks>
/// <param name="nombreParametro">Descripción del parámetro</param>
/// <returns>Descripción del valor de retorno</returns>
/// <response code="200">Descripción de respuesta exitosa</response>
/// <response code="400">Descripción de error</response>
```

### Ejemplo Real

```csharp
/// <summary>
/// Autentica un usuario con email/usuario y contraseña.
/// </summary>
/// <param name="loginDto">Credenciales del usuario</param>
/// <returns>JWT token de acceso y detalles del usuario</returns>
/// <remarks>
/// Ejemplo de solicitud:
/// 
///     POST /api/v1/auth/login
///     Content-Type: application/json
///     
///     {
///       "emailOrUsername": "john@example.com",
///       "password": "SecurePassword123"
///     }
/// </remarks>
/// <response code="200">Autenticación exitosa</response>
/// <response code="400">Email/usuario o contraseña inválida</response>
/// <response code="429">Demasiados intentos de login</response>
[HttpPost("login")]
public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
{
    // implementación
}
```

---

## Archivos Relacionados

### En el Proyecto

1. **ServiceCollectionExtensions.cs** - Configuración de Swagger
   - Definición de versión API
   - Configuración de autenticación Bearer
   - Inclusión de comentarios XML

2. **Program.cs** - Configuración de Swagger UI
   - `UseSwagger()` - Genera endpoint JSON de OpenAPI
   - `UseSwaggerUI()` - Interfaz interactivo

3. **Todos los Controllers** - Comentarios de endpoints
   - AuthController.cs
   - UsersController.cs
   - HealthController.cs

4. **Todos los DTOs** - Comentarios de modelos
   - LoginDto.cs
   - RegisterDto.cs
   - AuthResponseDto.cs
   - UserResponseDto.cs
   - Y más...

### Generación de Documentación

Se habilitó automáticamente en **AuthService_GR.Api.csproj**:

```xml
<GenerateDocumentationFile>true</GenerateDocumentationFile>
<DocumentationFile>bin\Debug\net8.0\AuthService_GR.Api.xml</DocumentationFile>
```

---

## Acceso a Swagger

### Desarrollo

```
http://localhost:5000/
```

Swagger UI se configura automáticamente para mostrarse en la raíz durante desarrollo.

### Producción

En producción, Swagger se desactiva por seguridad:

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(...);
}
```

Si deseas habilitarlo en producción, modifica `Program.cs`:

```csharp
app.UseSwagger();
app.UseSwaggerUI(...);
```

---

## Características Configuradas

### ✅ Información General
- Título y versión
- Descripción del servicio
- Contacto de soporte
- Licencia

### ✅ Autenticación
- Esquema Bearer Token
- Descripción de obtención de token
- Requisitos de autorización por endpoint

### ✅ Códigos de Respuesta
- 200 OK
- 201 Created
- 400 Bad Request
- 401 Unauthorized
- 403 Forbidden
- 404 Not Found
- 429 Too Many Requests
- 503 Service Unavailable

### ✅ Documentación
- Descripción de cada endpoint
- Ejemplos de solicitud/respuesta
- Parámetros documentados
- Validaciones descritas

### ✅ Modelos
- Esquemas de DTOs
- Propiedades documentadas
- Validaciones visible

---

## Configuración Actual en ServiceCollectionExtensions.cs

```csharp
public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
{
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen(options =>
    {
        // Información general
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "AuthService API",
            Version = "1.0.0",
            Description = "API de autenticación y gestión de usuarios...",
            Contact = new OpenApiContact
            {
                Name = "KinalSports Development Team",
                Email = "support@kinalsports.com",
                Url = new Uri("https://kinalsports.com")
            },
            License = new OpenApiLicense
            {
                Name = "MIT",
                Url = new Uri("https://opensource.org/licenses/MIT")
            }
        });

        // Autenticación Bearer
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Ingresa un JWT Token válido..."
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] { }
            }
        });

        // Incluir comentarios XML
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }

        options.EnableAnnotations();
    });

    return services;
}
```

---

## Configuración en Program.cs

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "AuthService API v1.0");
        options.RoutePrefix = string.Empty; // Mostrar en raíz
        options.DisplayOperationId();
        options.DefaultModelsExpandDepth(2);
        options.DocExpansion(DocExpansion.List);
        options.EnableFilter();
        options.ShowCommonExtensions();
    });
}
```

---

## Opciones de Swagger UI

| Opción | Valor | Descripción |
|--------|-------|-------------|
| SwaggerEndpoint | `/swagger/v1/swagger.json` | Ubicación del documento OpenAPI |
| RoutePrefix | `string.Empty` | Mostrar Swagger en raíz (/) |
| DisplayOperationId | true | Mostrar IDs de operación |
| DefaultModelsExpandDepth | 2 | Expandir modelos hasta nivel 2 |
| DocExpansion | List | Modo de expansión de documentación |
| EnableFilter | true | Habilitar filtro de búsqueda |
| ShowCommonExtensions | true | Mostrar extensiones comunes |

---

## Mejores Prácticas Implementadas

### ✅ Documentación Completa
- Cada endpoint tiene descripción detallada
- Ejemplos de solicitud/respuesta
- Descripción de parámetros
- Códigos de respuesta documentados

### ✅ Seguridad
- Autenticación Bearer visible
- Rate limiting documentado
- Validaciones descritas

### ✅ Usabilidad
- Swagger UI en raíz (acceso rápido)
- Filtro de búsqueda habilitado
- Modelos expandibles
- Descripción de contacto

### ✅ Mantenibilidad
- Comentarios XML en código
- Documentación centralizada
- Fácil de actualizar
- Versionado de API

---

## Ampliación Futura

Para agregar más endpoints o mejorar la documentación:

1. **Agregar comentarios XML** al nuevo código
2. **Reconstruir el proyecto** para generar el archivo XML
3. **Los cambios aparecerán automáticamente** en Swagger

Ejemplo:

```csharp
/// <summary>
/// Mi nuevo endpoint
/// </summary>
/// <param name="id">ID del recurso</param>
/// <returns>El recurso solicitado</returns>
/// <response code="200">Éxito</response>
[HttpGet("{id}")]
public async Task<ActionResult<MyDto>> GetById(string id)
{
    // implementación
}
```

---

## Troubleshooting

### Swagger no muestra comentarios XML

**Solución:**
1. Verifica que `GenerateDocumentationFile` esté habilitado en `.csproj`
2. Reconstruye el proyecto: `dotnet build`
3. Verifica que el archivo `.xml` exista en `bin/Debug/net8.0/`
4. Reinicia la aplicación

### Swagger está vacío o no funciona

**Solución:**
1. Verifica que controllers estén correctamente decorados con `[ApiController]`
2. Verifica que los métodos tengan `[HttpGet]`, `[HttpPost]`, etc.
3. Revisa la consola para errores
4. Limpia y reconstruye: `dotnet clean && dotnet build`

### Bearer Token no funciona en Swagger

**Solución:**
1. Haz clic en "Authorize" en la esquina superior derecha
2. Asegúrate de pegar solo el token (sin "Bearer ")
3. Verifica que el token sea válido y no esté expirado

---

## Referencias

- **OpenAPI 3.0 Spec:** https://swagger.io/specification/
- **Swashbuckle GitHub:** https://github.com/domaindrivendev/Swashbuckle.AspNetCore
- **Microsoft Docs:** https://learn.microsoft.com/aspnet/core/tutorials/web-api-help-pages-using-swagger

---

**Documento creado:** 3 de Marzo de 2024  
**Última actualización:** 3 de Marzo de 2024  
**Versión:** 1.0.0
