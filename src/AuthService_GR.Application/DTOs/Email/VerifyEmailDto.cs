using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace AuthService_GR.Application.DTOs.Email;

/// <summary>
/// DTO para la solicitud de verificación de email.
/// </summary>
/// <remarks>
/// Contiene el token de verificación enviado al email del usuario.
/// </remarks>
public class VerifyEmailDto
{
    /// <summary>
    /// Token de verificación de email.
    /// </summary>
    /// <remarks>
    /// Token enviado al email del usuario después del registro.
    /// Generalmente tiene formato de JWT o string alfanumérico.
    /// </remarks>
    public string Token {get; set;} = string.Empty;
}