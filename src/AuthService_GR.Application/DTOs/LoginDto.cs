using System.ComponentModel.DataAnnotations;

namespace AuthService_GR.Application.DTOs;

/// <summary>
/// DTO para la solicitud de autenticación de usuario (login).
/// </summary>
/// <remarks>
/// Contiene las credenciales necesarias para autenticar un usuario.
/// El cliente puede proporcionar ya sea un email o un nombre de usuario.
/// </remarks>
public class LoginDto
{
    /// <summary>
    /// Email o nombre de usuario del usuario para autenticar.
    /// </summary>
    /// <remarks>
    /// Campo requerido. Puede ser el email registrado o el nombre de usuario único.
    /// Ejemplo: john@example.com o john_doe
    /// </remarks>
    [Required]
    public string EmailOrUsername { get; set;} = string.Empty;

    /// <summary>
    /// Contraseña del usuario.
    /// </summary>
    /// <remarks>
    /// Campo requerido. Debe ser la contraseña correcta registrada durante el signup.
    /// </remarks>
    [Required]
    public string Password {get; set;} = string.Empty;
}