namespace AuthService_GR.Application.DTOs;

/// <summary>
/// DTO para la respuesta de registro de usuario.
/// </summary>
/// <remarks>
/// Contiene los datos del usuario registrado y estado de verificación de email.
/// </remarks>
public class RegisterResponseDto
{
    /// <summary>
    /// Indica si el registro fue exitoso.
    /// </summary>
    public bool Success {get; set;} = false;

    /// <summary>
    /// Datos del usuario registrado.
    /// </summary>
    public UserResponseDto User {get; set;} = new();

    /// <summary>
    /// Mensaje descriptivo del resultado del registro.
    /// </summary>
    /// <remarks>
    /// Ejemplos: "Usuario registrado exitosamente", "El usuario ya existe"
    /// </remarks>
    public string Message {get; set; } = string.Empty;

    /// <summary>
    /// Indica si la verificación de email es requerida.
    /// </summary>
    /// <remarks>
    /// Si es verdadero, se ha enviado un email de verificación al usuario.
    /// </remarks>
    public bool EmailVerificationRequired {get; set;} = true;
}