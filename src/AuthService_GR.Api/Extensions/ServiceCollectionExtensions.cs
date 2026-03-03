using AuthService_GR.Application.Interfaces;
using AuthService_GR.Application.Services;
using AuthService_GR.Domain.Interfaces;
using AuthService_GR.Persistence.Repositories;
using AuthService_GR.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AuthService_GR.Api.Extensions;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationSerivces(this IServiceCollection services, IConfiguration configuration)
    {

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
                .UseSnakeCaseNamingConvention());
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserManagementService, UserManagementService>();
        services.AddScoped<IPasswordHashService, PasswordHashService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<ICloudinaryService, CloudinaryService>();
        services.AddScoped<IEmailService, EmailService>();

        services.AddHealthChecks();

        return services;
    }

    public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "AuthService API",
                Version = "1.0.0",
                Description = "API de autenticación y gestión de usuarios para KinalSports. Proporciona funcionalidades de registro, login, verificación de email, recuperación de contraseña y gestión de roles de usuarios.",
                Contact = new Microsoft.OpenApi.Models.OpenApiContact
                {
                    Name = "KinalSports Development Team",
                    Email = "support@kinalsports.com",
                    Url = new Uri("https://kinalsports.com")
                },
                License = new Microsoft.OpenApi.Models.OpenApiLicense
                {
                    Name = "MIT",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                }
            });

            // Agregar soporte para parámetros de autorización Bearer Token
            options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Ingresa un JWT Token válido para acceder a endpoints protegidos.\n\nFormato: Bearer {token}"
            });

            options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] { }
                }
            });

            // Incluir comentarios XML
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }

            // Configurar ejemplos de respuestas
            options.EnableAnnotations();
        });

        return services;
    }
}