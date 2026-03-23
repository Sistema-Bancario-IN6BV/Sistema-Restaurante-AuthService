namespace AuthService_GR.Application.DTOs;

/// <summary>
/// DTO con información completa del usuario.
/// </summary>
/// <remarks>
/// Contiene todos los detalles públicos del usuario excepto la contraseña.
/// </remarks>
public class UserResponseDto
{
    /// <summary>
    /// ID único del usuario.
    /// </summary>
    public string Id{get; set;} = string.Empty;

    /// <summary>
    /// Nombre del usuario.
    /// </summary>
    public string Name {get;set;} = string.Empty;

    /// <summary>
    /// Apellido del usuario.
    /// </summary>
    public string Surname {get; set;} = string.Empty;

    /// <summary>
    /// Nombre de usuario único.
    /// </summary>
    public string Username {get;set;} = string.Empty;

    /// <summary>
    /// Dirección de email del usuario.
    /// </summary>
    public string Email {get; set;} = string.Empty;

    /// <summary>
    /// URL de la foto de perfil del usuario.
    /// </summary>
    public string ProfilePicture {get; set;} = string.Empty;

    /// <summary>
    /// Número de teléfono del usuario.
    /// </summary>
    public string Phone {get; set;} = string.Empty;
    
    /// <summary>
    /// Rol del usuario en el sistema.
    /// </summary>
    /// <remarks>
    /// Valores posibles: USER, MODERATOR, PLATFORM_ADMIN
    /// </remarks>
    public string Role {get; set;} = string.Empty;

    /// <summary>
    /// Estado activo/inactivo de la cuenta.
    /// </summary>
    public bool Status {get; set;}

    /// <summary>
    /// Indica si el email ha sido verificado.
    /// </summary>
    public bool IsEmailVerified {get; set;}

    /// <summary>
    /// Fecha y hora de creación de la cuenta.
    /// </summary>
    /// <remarks>
    /// Formato: ISO 8601 UTC
    /// </remarks>
    public DateTime CreatedAt {get; set;}

    /// <summary>
    /// Fecha y hora de la última actualización de la cuenta.
    /// </summary>
    /// <remarks>
    /// Formato: ISO 8601 UTC
    /// </remarks>
    public DateTime UpdatedAt {get; set;}
}