using System.ComponentModel.DataAnnotations;
using AuthService_GR.Application.Interfaces;

namespace AuthService_GR.Application.DTOs;

/// <summary>
/// DTO para la solicitud de registro de nuevo usuario.
/// </summary>
/// <remarks>
/// Contiene los datos necesarios para crear una nueva cuenta de usuario.
/// La foto de perfil es opcional. El registro se realiza mediante multipart/form-data.
/// </remarks>
public class RegisterDto
{
    /// <summary>
    /// Nombre del usuario.
    /// </summary>
    /// <remarks>
    /// Campo requerido. Máximo 25 caracteres.
    /// Ejemplo: John
    /// </remarks>
    [Required]
    [MaxLength(25)]
    public string Name {get; set;} = string.Empty;
    
    /// <summary>
    /// Apellido del usuario.
    /// </summary>
    /// <remarks>
    /// Campo requerido. Máximo 25 caracteres.
    /// Ejemplo: Doe
    /// </remarks>
    [Required]
    [MaxLength(25)]
    public string Surname {get; set;} = string.Empty;

    /// <summary>
    /// Nombre de usuario único.
    /// </summary>
    /// <remarks>
    /// Campo requerido. Debe ser único en el sistema.
    /// Ejemplo: john_doe
    /// </remarks>
    [Required]
    public string Username {get; set;} = string.Empty;

    /// <summary>
    /// Dirección de correo electrónico del usuario.
    /// </summary>
    /// <remarks>
    /// Campo requerido. Debe ser un email válido y único en el sistema.
    /// Se utilizará para la verificación de email.
    /// Ejemplo: john@example.com
    /// </remarks>
    [Required]
    [EmailAddress]
    public string Email {get; set;} = string.Empty;

    /// <summary>
    /// Contraseña de la cuenta.
    /// </summary>
    /// <remarks>
    /// Campo requerido. Mínimo 8 caracteres.
    /// Se recomienda usar una combinación de mayúsculas, minúsculas, números y caracteres especiales.
    /// </remarks>
    [Required]
    [MinLength(8)]
    public string Password {get; set;} = string.Empty;

    /// <summary>
    /// Número de teléfono del usuario.
    /// </summary>
    /// <remarks>
    /// Campo requerido. Exactamente 8 caracteres.
    /// Ejemplo: 12345678
    /// </remarks>
    [Required]
    [StringLength(8, MinimumLength = 8)]
    public string Phone {get; set;} = string.Empty;

    /// <summary>
    /// Foto de perfil del usuario (opcional).
    /// </summary>
    /// <remarks>
    /// Campo opcional. Archivo de imagen en formato multipart/form-data.
    /// Tamaño máximo: 10MB.
    /// Formatos soportados: JPG, PNG, GIF, WebP.
    /// </remarks>
    public IFileData? ProfilePicture{get; set;}
}