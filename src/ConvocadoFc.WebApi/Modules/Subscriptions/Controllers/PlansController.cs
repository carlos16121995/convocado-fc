using ConvocadoFc.Application.Handlers.Modules.Subscriptions.Interfaces;
using ConvocadoFc.Application.Handlers.Modules.Subscriptions.Models;
using ConvocadoFc.Domain.Models.Modules.Users.Identity;
using ConvocadoFc.Domain.Shared;
using ConvocadoFc.WebApi.Modules.Subscriptions.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConvocadoFc.WebApi.Modules.Subscriptions.Controllers;

/// <summary>
/// Endpoints de gerenciamento de planos de assinatura.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = SystemRoles.Master, Policy = AuthPolicies.EmailConfirmed)]
public sealed class PlansController(IPlanManagementHandler planHandler) : ControllerBase
{
    private readonly IPlanManagementHandler _planHandler = planHandler;

    /// <summary>
    /// Lista planos disponíveis para contratação.
    /// Inclui capacidade e status de ativação.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ListPlans(CancellationToken cancellationToken)
    {
        var plans = await _planHandler.ListPlansAsync(cancellationToken);

        return Ok(new ApiResponse<IReadOnlyCollection<PlanResponse>>
        {
            StatusCode = StatusCodes.Status200OK,
            Success = true,
            Message = "Planos disponíveis.",
            Data = plans.Select(MapToResponse).ToList()
        });
    }

    /// <summary>
    /// Obtém um plano por id.
    /// Retorna detalhes completos do plano.
    /// </summary>
    [HttpGet("{planId:guid}")]
    public async Task<IActionResult> GetPlan(Guid planId, CancellationToken cancellationToken)
    {
        var plan = await _planHandler.GetPlanAsync(planId, cancellationToken);
        if (plan is null)
        {
            return NotFound(new ApiResponse
            {
                StatusCode = StatusCodes.Status404NotFound,
                Success = false,
                Message = "Plano não encontrado."
            });
        }

        return Ok(new ApiResponse<PlanResponse>
        {
            StatusCode = StatusCodes.Status200OK,
            Success = true,
            Message = "Plano encontrado.",
            Data = MapToResponse(plan)
        });
    }

    /// <summary>
    /// Cria um novo plano.
    /// Valida código, nome e capacidade.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreatePlan([FromBody] CreatePlanRequest request, CancellationToken cancellationToken)
    {
        var result = await _planHandler.CreatePlanAsync(new CreatePlanCommand(
            request.Name,
            request.Code,
            request.Price,
            request.Currency,
            request.MaxTeams,
            request.MaxMembersPerTeam,
            request.IsCustomPricing,
            request.IsActive),
            cancellationToken);

        return result.Status switch
        {
            EPlanOperationStatus.CodeAlreadyExists => Conflict(ToError(StatusCodes.Status409Conflict, "Código de plano já existente.")),
            EPlanOperationStatus.NameAlreadyExists => Conflict(ToError(StatusCodes.Status409Conflict, "Nome de plano já existente.")),
            EPlanOperationStatus.InvalidCapacity => BadRequest(ToError(StatusCodes.Status400BadRequest, "Capacidade do plano inválida.")),
            _ => Ok(new ApiResponse<PlanResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Success = true,
                Message = "Plano criado com sucesso.",
                Data = MapToResponse(result.Plan!)
            })
        };
    }

    /// <summary>
    /// Atualiza um plano existente.
    /// Permite alterar preço, capacidade e status.
    /// </summary>
    [HttpPut("{planId:guid}")]
    public async Task<IActionResult> UpdatePlan(Guid planId, [FromBody] UpdatePlanRequest request, CancellationToken cancellationToken)
    {
        var result = await _planHandler.UpdatePlanAsync(new UpdatePlanCommand(
            planId,
            request.Name,
            request.Code,
            request.Price,
            request.Currency,
            request.MaxTeams,
            request.MaxMembersPerTeam,
            request.IsCustomPricing,
            request.IsActive),
            cancellationToken);

        return result.Status switch
        {
            EPlanOperationStatus.NotFound => NotFound(ToError(StatusCodes.Status404NotFound, "Plano não encontrado.")),
            EPlanOperationStatus.CodeAlreadyExists => Conflict(ToError(StatusCodes.Status409Conflict, "Código de plano já existente.")),
            EPlanOperationStatus.NameAlreadyExists => Conflict(ToError(StatusCodes.Status409Conflict, "Nome de plano já existente.")),
            EPlanOperationStatus.InvalidCapacity => BadRequest(ToError(StatusCodes.Status400BadRequest, "Capacidade do plano inválida.")),
            _ => Ok(new ApiResponse<PlanResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Success = true,
                Message = "Plano atualizado com sucesso.",
                Data = MapToResponse(result.Plan!)
            })
        };
    }

    /// <summary>
    /// Desativa um plano.
    /// Impede novas assinaturas com ele.
    /// </summary>
    [HttpDelete("{planId:guid}")]
    public async Task<IActionResult> DeactivatePlan(Guid planId, CancellationToken cancellationToken)
    {
        var result = await _planHandler.DeactivatePlanAsync(planId, cancellationToken);
        if (result.Status == EPlanOperationStatus.NotFound)
        {
            return NotFound(ToError(StatusCodes.Status404NotFound, "Plano não encontrado."));
        }

        return Ok(new ApiResponse<PlanResponse>
        {
            StatusCode = StatusCodes.Status200OK,
            Success = true,
            Message = "Plano desativado com sucesso.",
            Data = MapToResponse(result.Plan!)
        });
    }

    private static PlanResponse MapToResponse(PlanDto plan)
        => new PlanResponse(
            plan.Id,
            plan.Name,
            plan.Code,
            plan.Price,
            plan.Currency,
            plan.IsActive,
            plan.IsCustomPricing,
            new PlanCapacityResponse(plan.Capacity.MaxTeams, plan.Capacity.MaxMembersPerTeam));

    private static ApiResponse ToError(int statusCode, string message)
        => new ApiResponse
        {
            StatusCode = statusCode,
            Success = false,
            Message = message
        };
}
