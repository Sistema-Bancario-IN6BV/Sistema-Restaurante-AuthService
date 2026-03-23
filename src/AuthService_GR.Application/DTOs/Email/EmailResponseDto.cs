using System.ComponentModel.DataAnnotations;

namespace AuthService_GR.Application.DTOs.Email;

/// <summary>
/// DTO para las respuestas de operaciones relacionadas con email.
/// </summary>
/// <remarks>
/// Utilizado en endpoints de verificación, recuperación de contraseña y reenvío de emails.
/// </remarks>
public class EmailResponseDto
{
    /// <summary>
    /// Indica si la operación de email fue exitosa.
    /// </summary>
    public bool Success {get; set;}

    /// <summary>
    /// Mensaje descriptivo de la operación.
    /// </summary>
    /// <remarks>
    /// Ejemplos: "Email verificado exitosamente", "Token inválido o expirado"
    /// </remarks>
    public string Message {get; set;} = string.Empty;

    /// <summary>
    /// Datos adicionales de la respuesta (opcional).
    /// </summary>
    public object? Data {get; set;}
}