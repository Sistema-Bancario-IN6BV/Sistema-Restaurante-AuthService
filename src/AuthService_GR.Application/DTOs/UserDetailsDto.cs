namespace AuthService_GR.Application.DTOs;

/// <summary>
/// DTO con información resumida del usuario.
/// </summary>
/// <remarks>
/// Contiene los detalles básicos del usuario después de la autenticación.
/// </remarks>
public class UserDetailsDto
{
    /// <summary>
    /// ID único del usuario.
    /// </summary>
    public string Id {get; set;} = string.Empty;

    /// <summary>
    /// Nombre de usuario único.
    /// </summary>
    public string Username {get; set; } = string.Empty;

    /// <summary>
    /// URL de la foto de perfil del usuario.
    /// </summary>
    public string ProfilePicture {get; set; } = string.Empty;

    /// <summary>
    /// Rol del usuario en el sistema.
    /// </summary>
    /// <remarks>
    /// Valores posibles: USER, MODERATOR, PLATFORM_ADMIN
    /// </remarks>
    public string Role {get; set;} = string.Empty;
}