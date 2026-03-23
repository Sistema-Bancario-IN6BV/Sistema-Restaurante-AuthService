using System.ComponentModel.DataAnnotations;

namespace AuthService_GR.Application.DTOs.Email;

/// <summary>
/// DTO para la solicitud de reenvío de email de verificación.
/// </summary>
/// <remarks>
/// El usuario puede solicitar un nuevo email de verificación si no recibió el primero
/// o si el token expiró.
/// </remarks>
public class ResendVerificationDto
{
    /// <summary>
    /// Dirección de email del usuario.
    /// </summary>
    /// <remarks>
    /// Campo requerido. Debe ser el email registrado en la cuenta.
    /// </remarks>
    [Required]
    [EmailAddress]
    public string Email {get; set; } = string.Empty;
}