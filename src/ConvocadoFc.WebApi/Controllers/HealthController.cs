using ConvocadoFc.Domain.Shared;

using Microsoft.AspNetCore.Mvc;

namespace ConvocadoFc.WebApi.Controllers;

/// <summary>
/// Endpoint de verificação de saúde do serviço.
/// Indica disponibilidade básica da API.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Retorna o status do serviço.
    /// Útil para monitoramento e readiness checks.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public ActionResult<ApiResponse> Get() => Ok(new ApiResponse
    {
        StatusCode = StatusCodes.Status200OK,
        Success = true,
        Message = "Serviço operacional"
    });
}
