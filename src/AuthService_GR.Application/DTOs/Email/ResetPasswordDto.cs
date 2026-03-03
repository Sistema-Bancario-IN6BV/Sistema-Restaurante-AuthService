using System.ComponentModel.DataAnnotations;

namespace AuthService_GR.Application.DTOs.Email;

/// <summary>
/// DTO para la solicitud de restablecimiento de contraseña.
/// </summary>
/// <remarks>
/// Contiene el token de recuperación y la nueva contraseña que el usuario desea establecer.
/// </remarks>
public class ResetPasswordDto
{
    /// <summary>
    /// Token de restablecimiento de contraseña.
    /// </summary>
    /// <remarks>
    /// Token enviado al email del usuario a través del endpoint forgot-password.
    /// Tiene una validez limitada en el tiempo.
    /// </remarks>
    public string Token {get; set;} = string.Empty;
    
    /// <summary>
    /// Nueva contraseña para la cuenta.
    /// </summary>
    /// <remarks>
    /// Campo requerido. Mínimo 8 caracteres.
    /// Se recomienda usar una combinación de mayúsculas, minúsculas, números y caracteres especiales.
    /// </remarks>
    [Required]
    [MinLength(8)]
    public string NewPassword {get; set;} = string.Empty;
}