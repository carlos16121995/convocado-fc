using ConvocadoFc.Domain.Shared;
using Microsoft.AspNetCore.Mvc;

namespace ConvocadoFc.WebApi.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public ActionResult<ApiResponse> Get() => Ok(new ApiResponse
    {
        StatusCode = StatusCodes.Status200OK,
        Success = true,
        Message = "Serviço operacional"
    });
}
