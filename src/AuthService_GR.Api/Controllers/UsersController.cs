using AuthService_GR.Application.DTOs;
using AuthService_GR.Application.Interfaces;
using AuthService_GR.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AuthService_GR.Api.Controllers;

/// <summary>
/// Controlador para la gestión de usuarios y sus roles.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class UsersController(IUserManagementService userManagementService) : ControllerBase
{
    private async Task<bool> CurrentUserIsAdmin()
    {
        var userId = User.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
        if (string.IsNullOrEmpty(userId)) return false;
        var roles = await userManagementService.GetUserRolesAsync(userId);
        return roles.Contains(RoleConstants.PLATFORM_ADMIN);
    }

    /// <summary>
    /// Actualiza el rol de un usuario específico (solo administradores).
    /// </summary>
    /// <param name="userId">ID del usuario cuyo rol será actualizado.</param>
    /// <param name="dto">Objeto con el nuevo rol a asignar.</param>
    /// <returns>Datos actualizados del usuario.</returns>
    /// <remarks>
    /// Ejemplo de solicitud:
    /// 
    ///     PUT /api/v1/users/{userId}/role
    ///     Authorization: Bearer {token}
    ///     Content-Type: application/json
    ///     
    ///     {
    ///       "roleName": "PLATFORM_ADMIN"
    ///     }
    ///
    /// Ejemplo de respuesta exitosa:
    /// 
    ///     {
    ///       "id": "550e8400-e29b-41d4-a716-446655440000",
    ///       "name": "John",
    ///       "surname": "Doe",
    ///       "username": "john_doe",
    ///       "email": "john@example.com",
    ///       "profilePicture": "https://example.com/profile.jpg",
    ///       "phone": "12345678",
    ///       "role": "PLATFORM_ADMIN",
    ///       "status": true,
    ///       "isEmailVerified": true,
    ///       "createdAt": "2024-03-01T10:30:00Z",
    ///       "updatedAt": "2024-03-03T10:30:00Z"
    ///     }
    /// </remarks>
    /// <response code="200">Rol actualizado exitosamente.</response>
    /// <response code="401">El usuario no está autenticado.</response>
    /// <response code="403">El usuario no tiene permisos de administrador.</response>
    /// <response code="404">Usuario no encontrado.</response>
    /// <response code="429">Demasiadas solicitudes. Intenta más tarde.</response>
    [HttpPut("{userId}/role")]
    [EnableRateLimiting("ApiPolicy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<UserResponseDto>> UpdateUserRole(string userId, [FromBody] UpdateUserRoleDto dto)
    {
        if (!await CurrentUserIsAdmin())
        {
            return StatusCode(403, new { success = false, message = "Forbidden" });
        }

        var result = await userManagementService.UpdateUserRoleAsync(userId, dto.RoleName);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene todos los roles asignados a un usuario específico.
    /// </summary>
    /// <param name="userId">ID del usuario del cual se obtendrán los roles.</param>
    /// <returns>Lista de roles del usuario.</returns>
    /// <remarks>
    /// Ejemplo de solicitud:
    /// 
    ///     GET /api/v1/users/{userId}/roles
    ///     Authorization: Bearer {token}
    ///
    /// Ejemplo de respuesta:
    /// 
    ///     [
    ///       "USER",
    ///       "MODERATOR"
    ///     ]
    /// </remarks>
    /// <response code="200">Lista de roles obtenida exitosamente.</response>
    /// <response code="401">El usuario no está autenticado.</response>
    /// <response code="404">Usuario no encontrado.</response>
    [HttpGet("{userId}/roles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<string>>> GetUserRoles(string userId)
    {
        var roles = await userManagementService.GetUserRolesAsync(userId);
        return Ok(roles);
    }

    /// <summary>
    /// Obtiene todos los usuarios que tienen un rol específico (solo administradores).
    /// </summary>
    /// <param name="roleName">Nombre del rol para filtrar usuarios.</param>
    /// <returns>Lista de usuarios con el rol especificado.</returns>
    /// <remarks>
    /// Ejemplo de solicitud:
    /// 
    ///     GET /api/v1/users/by-role/PLATFORM_ADMIN
    ///     Authorization: Bearer {token}
    ///
    /// Ejemplo de respuesta:
    /// 
    ///     [
    ///       {
    ///         "id": "550e8400-e29b-41d4-a716-446655440000",
    ///         "name": "John",
    ///         "surname": "Doe",
    ///         "username": "john_doe",
    ///         "email": "john@example.com",
    ///         "profilePicture": "https://example.com/profile.jpg",
    ///         "phone": "12345678",
    ///         "role": "PLATFORM_ADMIN",
    ///         "status": true,
    ///         "isEmailVerified": true,
    ///         "createdAt": "2024-03-01T10:30:00Z",
    ///         "updatedAt": "2024-03-03T10:30:00Z"
    ///       }
    ///     ]
    /// </remarks>
    /// <response code="200">Lista de usuarios obtenida exitosamente.</response>
    /// <response code="401">El usuario no está autenticado.</response>
    /// <response code="403">El usuario no tiene permisos de administrador.</response>
    /// <response code="429">Demasiadas solicitudes. Intenta más tarde.</response>
    [HttpGet("by-role/{roleName}")]
    [EnableRateLimiting("ApiPolicy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IReadOnlyList<UserResponseDto>>> GetUsersByRole(string roleName)
    {
        if (!await CurrentUserIsAdmin())
        {
            return StatusCode(403, new { success = false, message = "Forbidden" });
        }
        
        var users = await userManagementService.GetUsersByRolesAsync(roleName);
        return Ok(users);
    }
}
