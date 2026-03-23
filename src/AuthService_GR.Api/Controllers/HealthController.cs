using Microsoft.AspNetCore.Mvc;

namespace AuthService_GR.Api.Controllers;

/// <summary>
/// Controlador para verificar el estado de salud del servicio de autenticación.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Obtiene el estado de salud del servicio.
    /// </summary>
    /// <returns>Objeto con el estado actual del servicio.</returns>
    /// <remarks>
    /// Ejemplo de solicitud:
    /// 
    ///     GET /api/v1/health
    ///
    /// Ejemplo de respuesta:
    /// 
    ///     {
    ///       "status": "Healthy",
    ///       "timestamp": "2024-03-03T10:30:00.000Z",
    ///       "service": "KinalSports Authentication Service"
    ///     }
    /// </remarks>
    /// <response code="200">El servicio está operativo.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetHealth()
    {
        var response = new
        {
            status = "Healthy",
            timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
            service = "KinalSports Authentication Service"
        };

        return Ok(response);
    }
}
