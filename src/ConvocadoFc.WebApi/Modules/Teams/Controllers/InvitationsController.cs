using ConvocadoFc.Application.Handlers.Modules.Teams.Interfaces;
using ConvocadoFc.Application.Handlers.Modules.Teams.Models;
using ConvocadoFc.Domain.Models.Modules.Users.Identity;
using ConvocadoFc.Domain.Shared;
using ConvocadoFc.WebApi.Modules.Teams.Models;
using ConvocadoFc.WebApi.Authorization;
using ConvocadoFc.WebApi.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConvocadoFc.WebApi.Modules.Teams.Controllers;

/// <summary>
/// Endpoints de convites e solicitações de entrada em times.
/// Controla fluxos de ingresso e aprovação.
/// </summary>
[ApiController]
[Route("api")]
[Authorize(Policy = AuthPolicies.EmailConfirmed)]
public sealed class InvitationsController(ITeamInvitationHandler invitationHandler) : ControllerBase
{
    private readonly ITeamInvitationHandler _invitationHandler = invitationHandler;

    /// <summary>
    /// Lista convites enviados por um time.
    /// Retorna histórico paginado de convites criados.
    /// </summary>
    [HttpGet("teams/{teamId:guid}/invites")]
    [Authorize(Policy = TeamPolicies.TeamModerator)]
    public async Task<IActionResult> ListSentInvites([FromRoute] Guid teamId, [FromQuery] ListSentInvitesQueryModel query, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var isSystemAdmin = User.IsInRole(SystemRoles.Admin) || User.IsInRole(SystemRoles.Master);

        var result = await _invitationHandler.ListSentInvitesAsync(new ListSentInvitesQuery(
            new PaginationQuery
            {
                Page = query.Page,
                PageSize = query.PageSize,
                OrderBy = query.OrderBy
            },
            teamId,
            currentUserId,
            isSystemAdmin),
            cancellationToken);

        return Ok(new ApiResponse<PaginatedResult<InviteResponse>>
        {
            StatusCode = StatusCodes.Status200OK,
            Success = true,
            Message = "Convites enviados.",
            Data = new PaginatedResult<InviteResponse>
            {
                TotalItems = result.TotalItems,
                Items = result.Items.Select(MapToResponse).ToList()
            }
        });
    }

    /// <summary>
    /// Cria um convite para um time.
    /// Pode gerar link, QR Code ou enviar e-mail.
    /// </summary>
    [HttpPost("teams/{teamId:guid}/invites")]
    public async Task<IActionResult> CreateInvite([FromRoute] Guid teamId, [FromBody] CreateInviteRequest request, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var isSystemAdmin = User.IsInRole(SystemRoles.Admin) || User.IsInRole(SystemRoles.Master);

        var result = await _invitationHandler.CreateInviteAsync(new CreateInviteCommand(
            teamId,
            currentUserId,
            request.TargetUserId,
            request.TargetEmail,
            request.Channel,
            request.MaxUses,
            request.ExpiresAt,
            request.Message,
            isSystemAdmin),
            cancellationToken);

        return result.Status switch
        {
            ETeamInviteOperationStatus.TeamNotFound => NotFound(ToError(StatusCodes.Status404NotFound, "Time não encontrado.")),
            ETeamInviteOperationStatus.UserNotFound => NotFound(ToError(StatusCodes.Status404NotFound, "Usuário não encontrado.")),
            ETeamInviteOperationStatus.Forbidden => Forbid(),
            ETeamInviteOperationStatus.InvalidData => BadRequest(ToError(StatusCodes.Status400BadRequest, "Dados inválidos.")),
            _ => Ok(new ApiResponse<InviteResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Success = true,
                Message = "Convite criado com sucesso.",
                Data = MapToResponse(result.Invite!)
            })
        };
    }

    /// <summary>
    /// Lista convites recebidos pelo usuário autenticado.
    /// Inclui convites diretos e por link.
    /// </summary>
    [HttpGet("users/me/invites")]
    public async Task<IActionResult> ListMyInvites([FromQuery] ListMyInvitesQueryModel query, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var result = await _invitationHandler.ListMyInvitesAsync(new ListMyInvitesQuery(
            new PaginationQuery
            {
                Page = query.Page,
                PageSize = query.PageSize,
                OrderBy = query.OrderBy
            },
            currentUserId),
            cancellationToken);

        return Ok(new ApiResponse<PaginatedResult<InviteResponse>>
        {
            StatusCode = StatusCodes.Status200OK,
            Success = true,
            Message = "Meus convites.",
            Data = new PaginatedResult<InviteResponse>
            {
                TotalItems = result.TotalItems,
                Items = result.Items.Select(MapToResponse).ToList()
            }
        });
    }

    /// <summary>
    /// Aceita um convite.
    /// Cria vínculo do usuário com o time quando permitido.
    /// </summary>
    [HttpPatch("invites/{inviteId:guid}")]
    public async Task<IActionResult> AcceptInvite([FromRoute] Guid inviteId, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var result = await _invitationHandler.AcceptInviteAsync(new AcceptInviteCommand(inviteId, currentUserId), cancellationToken);

        return result.Status switch
        {
            ETeamInviteOperationStatus.NotFound => NotFound(ToError(StatusCodes.Status404NotFound, "Convite não encontrado.")),
            ETeamInviteOperationStatus.Forbidden => Forbid(),
            ETeamInviteOperationStatus.InviteExpired => Conflict(ToError(StatusCodes.Status409Conflict, "Convite expirado.")),
            ETeamInviteOperationStatus.MaxUsesReached => Conflict(ToError(StatusCodes.Status409Conflict, "Convite sem usos disponíveis.")),
            ETeamInviteOperationStatus.AlreadyMember => Conflict(ToError(StatusCodes.Status409Conflict, "Usuário já é membro do time.")),
            ETeamInviteOperationStatus.AlreadyProcessed => Conflict(ToError(StatusCodes.Status409Conflict, "Convite já processado.")),
            ETeamInviteOperationStatus.InvalidData => BadRequest(ToError(StatusCodes.Status400BadRequest, "Dados inválidos.")),
            _ => Ok(new ApiResponse<InviteResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Success = true,
                Message = "Convite aceito com sucesso.",
                Data = MapToResponse(result.Invite!)
            })
        };
    }

    /// <summary>
    /// Lista solicitações de entrada em um time.
    /// Filtra por status e retorna paginação.
    /// </summary>
    [HttpGet("teams/{teamId:guid}/join-requests")]
    [Authorize(Policy = TeamPolicies.TeamModerator)]
    public async Task<IActionResult> ListJoinRequests([FromRoute] Guid teamId, [FromQuery] ListJoinRequestsQueryModel query, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var isSystemAdmin = User.IsInRole(SystemRoles.Admin) || User.IsInRole(SystemRoles.Master);

        var result = await _invitationHandler.ListJoinRequestsAsync(new ListJoinRequestsQuery(
            new PaginationQuery
            {
                Page = query.Page,
                PageSize = query.PageSize,
                OrderBy = query.OrderBy
            },
            teamId,
            currentUserId,
            query.Status,
            isSystemAdmin),
            cancellationToken);

        return Ok(new ApiResponse<PaginatedResult<JoinRequestResponse>>
        {
            StatusCode = StatusCodes.Status200OK,
            Success = true,
            Message = "Solicitações encontradas.",
            Data = new PaginatedResult<JoinRequestResponse>
            {
                TotalItems = result.TotalItems,
                Items = result.Items.Select(MapToResponse).ToList()
            }
        });
    }

    /// <summary>
    /// Cria uma solicitação para entrar em um time.
    /// Pode referenciar convite ou origem pública.
    /// </summary>
    [HttpPost("teams/{teamId:guid}/join-requests")]
    public async Task<IActionResult> CreateJoinRequest([FromRoute] Guid teamId, [FromBody] CreateJoinRequestRequest request, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var result = await _invitationHandler.CreateJoinRequestAsync(new CreateJoinRequestCommand(
            teamId,
            currentUserId,
            request.Message,
            request.InviteId,
            request.Source),
            cancellationToken);

        return result.Status switch
        {
            ETeamJoinRequestOperationStatus.TeamNotFound => NotFound(ToError(StatusCodes.Status404NotFound, "Time não encontrado.")),
            ETeamJoinRequestOperationStatus.UserNotFound => NotFound(ToError(StatusCodes.Status404NotFound, "Usuário não encontrado.")),
            ETeamJoinRequestOperationStatus.InviteNotFound => NotFound(ToError(StatusCodes.Status404NotFound, "Convite não encontrado.")),
            ETeamJoinRequestOperationStatus.InviteExpired => Conflict(ToError(StatusCodes.Status409Conflict, "Convite expirado.")),
            ETeamJoinRequestOperationStatus.MaxUsesReached => Conflict(ToError(StatusCodes.Status409Conflict, "Convite sem usos disponíveis.")),
            ETeamJoinRequestOperationStatus.AlreadyMember => Conflict(ToError(StatusCodes.Status409Conflict, "Usuário já é membro do time.")),
            ETeamJoinRequestOperationStatus.AlreadyProcessed => Conflict(ToError(StatusCodes.Status409Conflict, "Já existe solicitação pendente.")),
            ETeamJoinRequestOperationStatus.InvalidData => BadRequest(ToError(StatusCodes.Status400BadRequest, "Dados inválidos.")),
            _ => Ok(new ApiResponse<JoinRequestResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Success = true,
                Message = "Solicitação criada com sucesso.",
                Data = MapToResponse(result.Request!)
            })
        };
    }

    /// <summary>
    /// Revisa (aprova ou rejeita) uma solicitação de entrada.
    /// Atualiza status e registra quem realizou a revisão.
    /// </summary>
    [HttpPatch("join-requests/{requestId:guid}")]
    [Authorize(Policy = TeamPolicies.TeamModerator)]
    public async Task<IActionResult> ReviewJoinRequest([FromRoute] Guid requestId, [FromBody] ReviewJoinRequestRequest request, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var isSystemAdmin = User.IsInRole(SystemRoles.Admin) || User.IsInRole(SystemRoles.Master);

        var result = await _invitationHandler.ReviewJoinRequestAsync(new ReviewJoinRequestCommand(
            requestId,
            currentUserId,
            request.Approve,
            isSystemAdmin),
            cancellationToken);

        return result.Status switch
        {
            ETeamJoinRequestOperationStatus.NotFound => NotFound(ToError(StatusCodes.Status404NotFound, "Solicitação não encontrada.")),
            ETeamJoinRequestOperationStatus.Forbidden => Forbid(),
            ETeamJoinRequestOperationStatus.AlreadyProcessed => Conflict(ToError(StatusCodes.Status409Conflict, "Solicitação já processada.")),
            ETeamJoinRequestOperationStatus.InvalidData => BadRequest(ToError(StatusCodes.Status400BadRequest, "Dados inválidos.")),
            _ => Ok(new ApiResponse<JoinRequestResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Success = true,
                Message = request.Approve ? "Solicitação aprovada." : "Solicitação recusada.",
                Data = MapToResponse(result.Request!)
            })
        };
    }

    private static InviteResponse MapToResponse(TeamInviteDto invite)
        => new InviteResponse(
            invite.Id,
            invite.TeamId,
            invite.CreatedByUserId,
            invite.TargetUserId,
            invite.TargetEmail,
            invite.Token,
            invite.Channel,
            invite.Status,
            invite.IsPreApproved,
            invite.MaxUses,
            invite.UseCount,
            invite.Message,
            invite.CreatedAt,
            invite.ExpiresAt,
            invite.AcceptedAt);

    private static JoinRequestResponse MapToResponse(TeamJoinRequestDto request)
        => new JoinRequestResponse(
            request.Id,
            request.TeamId,
            request.UserId,
            request.InviteId,
            request.ReviewedByUserId,
            request.Status,
            request.Source,
            request.IsAutoApproved,
            request.Message,
            request.RequestedAt,
            request.ReviewedAt);

    private static ApiResponse ToError(int statusCode, string message)
        => new ApiResponse
        {
            StatusCode = statusCode,
            Success = false,
            Message = message
        };
}
