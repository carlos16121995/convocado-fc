using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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
public sealed class SubscriptionsController(ISubscriptionManagementHandler subscriptionHandler) : ControllerBase
{
    private readonly ISubscriptionManagementHandler _subscriptionHandler = subscriptionHandler;

    [HttpGet]
    public async Task<IActionResult> ListSubscriptions([FromQuery] ListSubscriptionsQueryModel query, CancellationToken cancellationToken)
    {
        var result = await _subscriptionHandler.ListSubscriptionsAsync(
            new ListSubscriptionsQuery(query.UserId, query.Status, query.PlanId),
            cancellationToken);

        return Ok(new ApiResponse<IReadOnlyCollection<SubscriptionResponse>>
        {
            StatusCode = StatusCodes.Status200OK,
            Success = true,
            Message = "Assinaturas encontradas.",
            Data = result.Select(MapToResponse).ToList()
        });
    }

    [HttpGet("users")]
    public async Task<IActionResult> ListSubscribedUsers([FromQuery] ListSubscribedUsersQueryModel query, CancellationToken cancellationToken)
    {
        var result = await _subscriptionHandler.ListSubscribedUsersAsync(
            new ListSubscribedUsersQuery(new PaginationQuery
            {
                Page = query.Page,
                PageSize = query.PageSize,
                OrderBy = query.OrderBy
            }, query.Status),
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

    [HttpPost("assign")]
    public async Task<IActionResult> AssignSubscription([FromBody] AssignSubscriptionRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var currentUserId))
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
            SubscriptionOperationStatus.UserNotFound => NotFound(ToError(StatusCodes.Status404NotFound, "Usuário não encontrado.")),
            SubscriptionOperationStatus.PlanNotFound => NotFound(ToError(StatusCodes.Status404NotFound, "Plano não encontrado.")),
            SubscriptionOperationStatus.ActiveSubscriptionExists => Conflict(ToError(StatusCodes.Status409Conflict, "Usuário já possui assinatura ativa.")),
            _ => Ok(new ApiResponse<SubscriptionResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Success = true,
                Message = "Assinatura atribuída com sucesso.",
                Data = MapToResponse(result.Subscription!)
            })
        };
    }

    [HttpPost("change")]
    public async Task<IActionResult> ChangeSubscription([FromBody] ChangeSubscriptionRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var result = await _subscriptionHandler.ChangeSubscriptionAsync(new ChangeSubscriptionCommand(
            request.SubscriptionId,
            request.PlanId,
            request.EndsAt,
            request.AutoRenew,
            request.Status,
            currentUserId,
            request.Note),
            cancellationToken);

        return result.Status switch
        {
            SubscriptionOperationStatus.NotFound => NotFound(ToError(StatusCodes.Status404NotFound, "Assinatura não encontrada.")),
            SubscriptionOperationStatus.PlanNotFound => NotFound(ToError(StatusCodes.Status404NotFound, "Plano não encontrado.")),
            _ => Ok(new ApiResponse<SubscriptionResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Success = true,
                Message = "Assinatura atualizada com sucesso.",
                Data = MapToResponse(result.Subscription!)
            })
        };
    }

    [HttpPost("remove")]
    public async Task<IActionResult> RemoveSubscription([FromBody] RemoveSubscriptionRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var result = await _subscriptionHandler.RemoveSubscriptionAsync(new RemoveSubscriptionCommand(
            request.SubscriptionId,
            currentUserId,
            request.Note),
            cancellationToken);

        return result.Status switch
        {
            SubscriptionOperationStatus.NotFound => NotFound(ToError(StatusCodes.Status404NotFound, "Assinatura não encontrada.")),
            SubscriptionOperationStatus.SubscriptionNotActive => Conflict(ToError(StatusCodes.Status409Conflict, "Assinatura não está ativa.")),
            _ => Ok(new ApiResponse<SubscriptionResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Success = true,
                Message = "Assinatura removida com sucesso.",
                Data = MapToResponse(result.Subscription!)
            })
        };
    }

    private bool TryGetUserId(out Guid userId)
    {
        userId = Guid.Empty;
        var rawId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return rawId is not null && Guid.TryParse(rawId, out userId);
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
