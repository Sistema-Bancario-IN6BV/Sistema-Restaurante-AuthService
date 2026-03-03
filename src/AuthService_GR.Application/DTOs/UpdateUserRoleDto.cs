namespace AuthService_GR.Application.DTOs;

/// <summary>
/// DTO para actualizar el rol de un usuario.
/// </summary>
/// <remarks>
/// Solo administradores pueden usar este endpoint.
/// </remarks>
public class UpdateUserRoleDto
{
    /// <summary>
    /// Nombre del nuevo rol a asignar.
    /// </summary>
    /// <remarks>
    /// Valores válidos: USER, MODERATOR, PLATFORM_ADMIN
    /// </remarks>
    public string RoleName {get; set;} = string.Empty;
}