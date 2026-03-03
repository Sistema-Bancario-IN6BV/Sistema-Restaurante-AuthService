namespace AuthService_GR.Application.DTOs;

/// <summary>
/// DTO para la respuesta de autenticación (login).
/// </summary>
/// <remarks>
/// Contiene el token JWT y los detalles del usuario autenticado.
/// </remarks>
public class AuthResponseDto
{
    /// <summary>
    /// Indica si la autenticación fue exitosa.
    /// </summary>
    /// <remarks>
    /// Valor por defecto: true
    /// </remarks>
    public bool Success {get; set;} = true;

    /// <summary>
    /// Mensaje descriptivo de la respuesta.
    /// </summary>
    /// <remarks>
    /// Ejemplos: "Autenticación exitosa", "Credenciales inválidas"
    /// </remarks>
    public string Message {get; set;} = string.Empty;

    /// <summary>
    /// Token JWT para acceder a endpoints protegidos.
    /// </summary>
    /// <remarks>
    /// Token de tipo Bearer que debe incluirse en el header Authorization.
    /// Formato: Authorization: Bearer {Token}
    /// </remarks>
    public string Token {get; set; } = string.Empty;

    /// <summary>
    /// Detalles del usuario autenticado.
    /// </summary>
    public UserDetailsDto UserDetails {get; set;} = new();

    /// <summary>
    /// Fecha y hora en que expira el token.
    /// </summary>
    /// <remarks>
    /// Formato: ISO 8601 UTC
    /// </remarks>
    public DateTime ExpiresAt {get; set;}
}