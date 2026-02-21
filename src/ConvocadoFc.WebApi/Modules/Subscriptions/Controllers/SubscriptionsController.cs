using ConvocadoFc.Application.Handlers.Modules.Subscriptions.Interfaces;
using ConvocadoFc.Application.Handlers.Modules.Subscriptions.Models;
using ConvocadoFc.Domain.Models.Modules.Users.Identity;
using ConvocadoFc.Domain.Shared;
using ConvocadoFc.WebApi.Modules.Subscriptions.Models;
using ConvocadoFc.WebApi.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConvocadoFc.WebApi.Modules.Subscriptions.Controllers;

/// <summary>
/// Endpoints de gerenciamento de assinaturas e acessos pagos.
/// </summary>
[ApiController]
[Route("api/subscriptions")]
[Authorize(Roles = SystemRoles.Master, Policy = AuthPolicies.EmailConfirmed)]
public sealed class SubscriptionsController(ISubscriptionManagementHandler subscriptionHandler) : ControllerBase
{
    private readonly ISubscriptionManagementHandler _subscriptionHandler = subscriptionHandler;

    /// <summary>
    /// Lista assinaturas existentes com filtros opcionais.
    /// Retorna paginação quando aplicável.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ListSubscriptions([FromQuery] ListSubscriptionsQueryModel query, CancellationToken cancellationToken)
    {
        var status = query.Status;
        var result = await _subscriptionHandler.ListSubscriptionsAsync(
            new ListSubscriptionsQuery(query.UserId, status, query.PlanId),
            cancellationToken);

        return Ok(new ApiResponse<IReadOnlyCollection<SubscriptionResponse>>
        {
            StatusCode = StatusCodes.Status200OK,
            Success = true,
            Message = "Assinaturas encontradas.",
            Data = result.Select(MapToResponse).ToList()
        });
    }

    /// <summary>
    /// Lista usuários com assinatura ativa ou em outros estados.
    /// Útil para gestão administrativa de acessos.
    /// </summary>
    [HttpGet("users")]
    public async Task<IActionResult> ListSubscribedUsers([FromQuery] ListSubscribedUsersQueryModel query, CancellationToken cancellationToken)
    {
        var status = query.Status;
        var result = await _subscriptionHandler.ListSubscribedUsersAsync(
            new ListSubscribedUsersQuery(new PaginationQuery
            {
                Page = query.Page,
                PageSize = query.PageSize,
                OrderBy = query.OrderBy
            }, status),
            cancellationToken);

        return Ok(new ApiResponse<PaginatedResult<SubscribedUserResponse>>
        {
            StatusCode = StatusCodes.Status200OK,
            Success = true,
            Message = "Usuários com assinatura.",
            Data = new PaginatedResult<SubscribedUserResponse>
            {
                TotalItems = result.TotalItems,
                Items = result.Items.Select(MapToResponse).ToList()
            }
        });
    }

    /// <summary>
    /// Cria uma assinatura para um usuário.
    /// Registra o plano, datas e política de renovação.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> AssignSubscription([FromBody] AssignSubscriptionRequest request, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var result = await _subscriptionHandler.AssignSubscriptionAsync(new AssignSubscriptionCommand(
            request.UserId,
            request.PlanId,
            request.StartsAt,
            request.EndsAt,
            request.AutoRenew,
            currentUserId,
            request.Note),
            cancellationToken);

        return result.Status switch
        {
            ESubscriptionOperationStatus.UserNotFound => NotFound(ToError(StatusCodes.Status404NotFound, "Usuário não encontrado.")),
            ESubscriptionOperationStatus.PlanNotFound => NotFound(ToError(StatusCodes.Status404NotFound, "Plano não encontrado.")),
            ESubscriptionOperationStatus.ActiveSubscriptionExists => Conflict(ToError(StatusCodes.Status409Conflict, "Usuário já possui assinatura ativa.")),
            _ => Ok(new ApiResponse<SubscriptionResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Success = true,
                Message = "Assinatura atribuída com sucesso.",
                Data = MapToResponse(result.Subscription!)
            })
        };
    }

    /// <summary>
    /// Atualiza uma assinatura existente.
    /// Permite alterar plano, datas, status e renovação.
    /// </summary>
    [HttpPatch("{subscriptionId:guid}")]
    public async Task<IActionResult> ChangeSubscription([FromRoute] Guid subscriptionId, [FromBody] ChangeSubscriptionRequest request, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var status = request.Status;

        var result = await _subscriptionHandler.ChangeSubscriptionAsync(new ChangeSubscriptionCommand(
            subscriptionId,
            request.PlanId,
            request.EndsAt,
            request.AutoRenew,
            status,
            currentUserId,
            request.Note),
            cancellationToken);

        return result.Status switch
        {
            ESubscriptionOperationStatus.NotFound => NotFound(ToError(StatusCodes.Status404NotFound, "Assinatura não encontrada.")),
            ESubscriptionOperationStatus.PlanNotFound => NotFound(ToError(StatusCodes.Status404NotFound, "Plano não encontrado.")),
            _ => Ok(new ApiResponse<SubscriptionResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Success = true,
                Message = "Assinatura atualizada com sucesso.",
                Data = MapToResponse(result.Subscription!)
            })
        };
    }

    /// <summary>
    /// Remove uma assinatura.
    /// Pode registrar nota administrativa no histórico.
    /// </summary>
    [HttpDelete("{subscriptionId:guid}")]
    public async Task<IActionResult> RemoveSubscription([FromRoute] Guid subscriptionId, [FromBody] RemoveSubscriptionRequest? request, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var result = await _subscriptionHandler.RemoveSubscriptionAsync(new RemoveSubscriptionCommand(
            subscriptionId,
            currentUserId,
            request?.Note),
            cancellationToken);

        return result.Status switch
        {
            ESubscriptionOperationStatus.NotFound => NotFound(ToError(StatusCodes.Status404NotFound, "Assinatura não encontrada.")),
            ESubscriptionOperationStatus.SubscriptionNotActive => Conflict(ToError(StatusCodes.Status409Conflict, "Assinatura não está ativa.")),
            _ => Ok(new ApiResponse<SubscriptionResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Success = true,
                Message = "Assinatura removida com sucesso.",
                Data = MapToResponse(result.Subscription!)
            })
        };
    }

    private static SubscriptionResponse MapToResponse(SubscriptionDto subscription)
        => new SubscriptionResponse(
            subscription.Id,
            subscription.OwnerUserId,
            subscription.PlanId,
            subscription.PlanName,
            subscription.PlanCode,
            subscription.Status,
            subscription.StartsAt,
            subscription.EndsAt,
            subscription.AutoRenew,
            new PlanCapacityResponse(subscription.Capacity.MaxTeams, subscription.Capacity.MaxMembersPerTeam));

    private static SubscribedUserResponse MapToResponse(SubscribedUserDto user)
        => new SubscribedUserResponse(
            user.UserId,
            user.Email,
            user.FullName,
            MapToResponse(user.Subscription));

    private static ApiResponse ToError(int statusCode, string message)
        => new ApiResponse
        {
            StatusCode = statusCode,
            Success = false,
            Message = message
        };
}
