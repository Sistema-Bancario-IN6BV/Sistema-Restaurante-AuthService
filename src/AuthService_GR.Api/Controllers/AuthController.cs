using System;
using AuthService_GR.Application.DTOs;
using AuthService_GR.Application.DTOs.Email;
using AuthService_GR.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AuthService_GR.Api.Controllers;

/// <summary>
/// Controlador de autenticación que gestiona registro, login, verificación de email y recuperación de contraseña.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    /// <summary>
    /// Obtiene el perfil del usuario autenticado actual.
    /// </summary>
    /// <returns>Objeto que contiene los datos del perfil del usuario autenticado.</returns>
    /// <remarks>
    /// Ejemplo de solicitud:
    /// 
    ///     GET /api/v1/auth/profile
    ///
    /// Ejemplo de respuesta exitosa:
    /// 
    ///     {
    ///       "success": true,
    ///       "message": "Perfil obtenido exitosamente",
    ///       "data": {
    ///         "id": "550e8400-e29b-41d4-a716-446655440000",
    ///         "username": "john_doe",
    ///         "profilePicture": "https://example.com/profile.jpg",
    ///         "role": "USER"
    ///       }
    ///     }
    /// </remarks>
    /// <response code="200">El perfil se obtuvo exitosamente.</response>
    /// <response code="401">El usuario no está autenticado.</response>
    /// <response code="404">El usuario no fue encontrado.</response>
    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> GetProfile()
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
        {
            return Unauthorized();
        }

        var user = await authService.GetUserByIdAsync(userIdClaim.Value);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(new
        {
            success = true,
            message = "Perfil obtenido exitosamente",
            data = user
        });
    }

    /// <summary>
    /// Registra un nuevo usuario en el sistema.
    /// </summary>
    /// <param name="registerDto">Datos requeridos para el registro (nombre, apellido, usuario, email, contraseña, teléfono, foto de perfil opcional).</param>
    /// <returns>Objeto con los datos del usuario registrado y token de verificación de email.</returns>
    /// <remarks>
    /// Ejemplo de solicitud (multipart/form-data):
    /// 
    ///     POST /api/v1/auth/register
    ///     Content-Type: multipart/form-data
    ///     
    ///     name=John
    ///     surname=Doe
    ///     username=john_doe
    ///     email=john@example.com
    ///     password=SecurePassword123
    ///     phone=12345678
    ///     profilePicture=[archivo de imagen]
    ///
    /// Ejemplo de respuesta exitosa (Status 201):
    /// 
    ///     {
    ///       "success": true,
    ///       "user": {
    ///         "id": "550e8400-e29b-41d4-a716-446655440000",
    ///         "name": "John",
    ///         "surname": "Doe",
    ///         "username": "john_doe",
    ///         "email": "john@example.com",
    ///         "phone": "12345678",
    ///         "profilePicture": "https://example.com/profile.jpg",
    ///         "role": "USER",
    ///         "status": true,
    ///         "isEmailVerified": false,
    ///         "createdAt": "2024-03-01T10:30:00Z",
    ///         "updatedAt": "2024-03-01T10:30:00Z"
    ///       },
    ///       "message": "Usuario registrado exitosamente. Se ha enviado un email de verificación.",
    ///       "emailVerificationRequired": true
    ///     }
    /// </remarks>
    /// <response code="201">Usuario registrado exitosamente.</response>
    /// <response code="400">Los datos proporcionados son inválidos o el usuario ya existe.</response>
    /// <response code="409">El email o usuario ya está registrado en el sistema.</response>
    /// <response code="503">Error al enviar el email de verificación.</response>
    [HttpPost("register")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    [EnableRateLimiting("AuthPolicy")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<RegisterResponseDto>> Register([FromForm] RegisterDto registerDto)
    {
        var result = await authService.RegisterAsync(registerDto);
        return StatusCode(201, result);
    }

    /// <summary>
    /// Autentica un usuario con email/usuario y contraseña.
    /// </summary>
    /// <param name="loginDto">Credenciales del usuario (email o usuario y contraseña).</param>
    /// <returns>JWT token de acceso y detalles del usuario autenticado.</returns>
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
    ///
    /// Ejemplo de respuesta exitosa:
    /// 
    ///     {
    ///       "success": true,
    ///       "message": "Autenticación exitosa",
    ///       "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    ///       "userDetails": {
    ///         "id": "550e8400-e29b-41d4-a716-446655440000",
    ///         "username": "john_doe",
    ///         "profilePicture": "https://example.com/profile.jpg",
    ///         "role": "USER"
    ///       },
    ///       "expiresAt": "2024-03-02T10:30:00Z"
    ///     }
    /// </remarks>
    /// <response code="200">Autenticación exitosa.</response>
    /// <response code="400">Email/usuario o contraseña inválida.</response>
    /// <response code="403">El usuario está inactivo o el email no ha sido verificado.</response>
    /// <response code="429">Demasiados intentos de login. Intenta más tarde.</response>
    [HttpPost("login")]
    [EnableRateLimiting("AuthPolicy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
    {
        var result = await authService.LoginAsync(loginDto);
        return Ok(result);
    }

    /// <summary>
    /// Verifica la dirección de email del usuario utilizando un token de verificación.
    /// </summary>
    /// <param name="verifyEmailDto">Token de verificación enviado al email del usuario.</param>
    /// <returns>Mensaje confirmando la verificación exitosa del email.</returns>
    /// <remarks>
    /// Ejemplo de solicitud:
    /// 
    ///     POST /api/v1/auth/verify-email
    ///     Content-Type: application/json
    ///     
    ///     {
    ///       "token": "verification_token_from_email"
    ///     }
    ///
    /// Ejemplo de respuesta exitosa:
    /// 
    ///     {
    ///       "success": true,
    ///       "message": "Email verificado exitosamente",
    ///       "data": null
    ///     }
    /// </remarks>
    /// <response code="200">Email verificado exitosamente.</response>
    /// <response code="400">Token inválido o expirado.</response>
    /// <response code="404">Usuario no encontrado.</response>
    /// <response code="429">Demasiados intentos. Intenta más tarde.</response>
    [HttpPost("verify-email")]
    [EnableRateLimiting("ApiPolicy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<EmailResponseDto>> VerifyEmail([FromBody] VerifyEmailDto verifyEmailDto)
    {
        var result = await authService.VerifyEmailAsync(verifyEmailDto);
        return Ok(result);
    }

    /// <summary>
    /// Reenvía un email de verificación a la dirección registrada del usuario.
    /// </summary>
    /// <param name="resendDto">Email del usuario para el cual se reenviará el token de verificación.</param>
    /// <returns>Mensaje confirmando el envío del email de verificación.</returns>
    /// <remarks>
    /// Ejemplo de solicitud:
    /// 
    ///     POST /api/v1/auth/resend-verification
    ///     Content-Type: application/json
    ///     
    ///     {
    ///       "email": "john@example.com"
    ///     }
    ///
    /// Ejemplo de respuesta exitosa:
    /// 
    ///     {
    ///       "success": true,
    ///       "message": "Email de verificación reenviado exitosamente",
    ///       "data": null
    ///     }
    /// </remarks>
    /// <response code="200">Email de verificación reenviado exitosamente.</response>
    /// <response code="400">El email ya ha sido verificado.</response>
    /// <response code="404">Usuario no encontrado.</response>
    /// <response code="429">Demasiados intentos. Intenta más tarde.</response>
    /// <response code="503">Error al enviar el email. Intenta más tarde.</response>
    [HttpPost("resend-verification")]
    [EnableRateLimiting("AuthPolicy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<EmailResponseDto>> ResendVerification([FromBody] ResendVerificationDto resendDto)
    {
        var result = await authService.ResendVerificationEmailAsync(resendDto);

        if (!result.Success)
        {
            if (result.Message.Contains("no encontrado", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(result);
            }
            if (result.Message.Contains("ya ha sido verificado", StringComparison.OrdinalIgnoreCase) ||
                result.Message.Contains("ya verificado", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(result);
            }
            return StatusCode(503, result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Inicia el proceso de recuperación de contraseña enviando un email con un token de restablecimiento.
    /// </summary>
    /// <param name="forgotPasswordDto">Email del usuario para el cual se iniciará la recuperación de contraseña.</param>
    /// <returns>Mensaje confirmando el envío del email de recuperación.</returns>
    /// <remarks>
    /// Ejemplo de solicitud:
    /// 
    ///     POST /api/v1/auth/forgot-password
    ///     Content-Type: application/json
    ///     
    ///     {
    ///       "email": "john@example.com"
    ///     }
    ///
    /// Ejemplo de respuesta:
    /// 
    ///     {
    ///       "success": true,
    ///       "message": "Email de recuperación de contraseña enviado. Por favor, revisa tu bandeja de entrada.",
    ///       "data": null
    ///     }
    ///
    /// Nota: Este endpoint siempre devuelve éxito por motivos de seguridad, incluso si el email no existe.
    /// </remarks>
    /// <response code="200">Email de recuperación enviado (respuesta exitosa siempre por seguridad).</response>
    /// <response code="429">Demasiados intentos. Intenta más tarde.</response>
    /// <response code="503">Error al enviar el email. Intenta más tarde.</response>
    [HttpPost("forgot-password")]
    [EnableRateLimiting("AuthPolicy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<EmailResponseDto>> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
    {
        var result = await authService.ForgotPasswordAsync(forgotPasswordDto);

        if (!result.Success)
        {
            return StatusCode(503, result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Restablece la contraseña del usuario utilizando un token válido de recuperación.
    /// </summary>
    /// <param name="resetPasswordDto">Token de recuperación y nueva contraseña.</param>
    /// <returns>Mensaje confirmando el restablecimiento exitoso de la contraseña.</returns>
    /// <remarks>
    /// Ejemplo de solicitud:
    /// 
    ///     POST /api/v1/auth/reset-password
    ///     Content-Type: application/json
    ///     
    ///     {
    ///       "token": "reset_token_from_email",
    ///       "newPassword": "NewSecurePassword123"
    ///     }
    ///
    /// Ejemplo de respuesta exitosa:
    /// 
    ///     {
    ///       "success": true,
    ///       "message": "Contraseña restablecida exitosamente",
    ///       "data": null
    ///     }
    /// </remarks>
    /// <response code="200">Contraseña restablecida exitosamente.</response>
    /// <response code="400">Token inválido, expirado o contraseña no válida.</response>
    /// <response code="404">Usuario no encontrado.</response>
    /// <response code="429">Demasiados intentos. Intenta más tarde.</response>
    [HttpPost("reset-password")]
    [EnableRateLimiting("AuthPolicy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<EmailResponseDto>> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
    {
        var result = await authService.ResetPasswordAsync(resetPasswordDto);
        return Ok(result);
    }
}
