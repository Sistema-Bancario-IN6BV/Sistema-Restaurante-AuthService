using System.ComponentModel.DataAnnotations;

namespace AuthService_GR.Application.DTOs.Email;

/// <summary>
/// DTO para la solicitud de recuperación de contraseña.
/// </summary>
/// <remarks>
/// El usuario debe proporcionar el email asociado a su cuenta.
/// Se enviará un email con un token para restablecimiento de contraseña.
/// </remarks>
public class ForgotPasswordDto
{
    /// <summary>
    /// Dirección de email de la cuenta.
    /// </summary>
    /// <remarks>
    /// Campo requerido. Debe ser el email registrado en la cuenta.
    /// Nota: La respuesta siempre será exitosa por motivos de seguridad,
    /// incluso si el email no existe en el sistema.
    /// </remarks>
    [Required]
    [EmailAddress]
    public string Email {get; set;} = string.Empty;
}