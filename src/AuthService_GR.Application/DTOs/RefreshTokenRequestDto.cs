using System.ComponentModel.DataAnnotations;

namespace AuthService_GR.Application.DTOs;

/// <summary>
/// DTO para solicitar la renovación del token de acceso, o para revocar
/// el refresh token (logout).
/// </summary>
public class RefreshTokenRequestDto
{
    /// <summary>
    /// Refresh token previamente emitido por /auth/login o /auth/refresh.
    /// </summary>
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
