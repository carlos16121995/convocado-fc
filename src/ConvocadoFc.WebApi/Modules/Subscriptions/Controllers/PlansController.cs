using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using ConvocadoFc.Application.Handlers.Modules.Subscriptions.Interfaces;
using ConvocadoFc.Application.Handlers.Modules.Subscriptions.Models;
using ConvocadoFc.Domain.Models.Modules.Users.Identity;
using ConvocadoFc.Domain.Shared;
using ConvocadoFc.WebApi.Modules.Subscriptions.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ConvocadoFc.WebApi.Modules.Subscriptions.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = SystemRoles.Master, Policy = AuthPolicies.EmailConfirmed)]
public sealed class PlansController(IPlanManagementHandler planHandler) : ControllerBase
{
    private readonly IPlanManagementHandler _planHandler = planHandler;

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
            PlanOperationStatus.CodeAlreadyExists => Conflict(ToError(StatusCodes.Status409Conflict, "Código de plano já existente.")),
            PlanOperationStatus.NameAlreadyExists => Conflict(ToError(StatusCodes.Status409Conflict, "Nome de plano já existente.")),
            PlanOperationStatus.InvalidCapacity => BadRequest(ToError(StatusCodes.Status400BadRequest, "Capacidade do plano inválida.")),
            _ => Ok(new ApiResponse<PlanResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Success = true,
                Message = "Plano criado com sucesso.",
                Data = MapToResponse(result.Plan!)
            })
        };
    }

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
            PlanOperationStatus.NotFound => NotFound(ToError(StatusCodes.Status404NotFound, "Plano não encontrado.")),
            PlanOperationStatus.CodeAlreadyExists => Conflict(ToError(StatusCodes.Status409Conflict, "Código de plano já existente.")),
            PlanOperationStatus.NameAlreadyExists => Conflict(ToError(StatusCodes.Status409Conflict, "Nome de plano já existente.")),
            PlanOperationStatus.InvalidCapacity => BadRequest(ToError(StatusCodes.Status400BadRequest, "Capacidade do plano inválida.")),
            _ => Ok(new ApiResponse<PlanResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Success = true,
                Message = "Plano atualizado com sucesso.",
                Data = MapToResponse(result.Plan!)
            })
        };
    }

    [HttpDelete("{planId:guid}")]
    public async Task<IActionResult> DeactivatePlan(Guid planId, CancellationToken cancellationToken)
    {
        var result = await _planHandler.DeactivatePlanAsync(planId, cancellationToken);
        if (result.Status == PlanOperationStatus.NotFound)
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
