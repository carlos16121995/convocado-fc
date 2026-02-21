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
/// Endpoints de gerenciamento de jogadores por time.
/// Inclui perfil, administração e remoção.
/// </summary>
[ApiController]
[Route("api/teams/{teamId:guid}/players")]
[Authorize(Policy = AuthPolicies.EmailConfirmed)]
public sealed class PlayersController(ITeamPlayerHandler playerHandler) : ControllerBase
{
    private readonly ITeamPlayerHandler _playerHandler = playerHandler;

    /// <summary>
    /// Lista jogadores do time.
    /// Retorna dados paginados e ordenados.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = TeamPolicies.TeamModerator)]
    public async Task<IActionResult> ListPlayers([FromRoute] Guid teamId, [FromQuery] ListPlayersQueryModel query, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var isSystemAdmin = User.IsInRole(SystemRoles.Admin) || User.IsInRole(SystemRoles.Master);

        var result = await _playerHandler.ListPlayersAsync(new ListTeamPlayersQuery(
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

        return Ok(new ApiResponse<PaginatedResult<PlayerResponse>>
        {
            StatusCode = StatusCodes.Status200OK,
            Success = true,
            Message = "Jogadores encontrados.",
            Data = new PaginatedResult<PlayerResponse>
            {
                TotalItems = result.TotalItems,
                Items = result.Items.Select(MapToResponse).ToList()
            }
        });
    }

    /// <summary>
    /// Atualiza o perfil do jogador autenticado no time.
    /// Permite definir posições e período de hiato.
    /// </summary>
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile([FromRoute] Guid teamId, [FromBody] UpdateMyProfileRequest request, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var result = await _playerHandler.UpdateMyProfileAsync(new UpdateMyProfileCommand(
            teamId,
            currentUserId,
            request.PrimaryPosition,
            request.SecondaryPosition,
            request.TertiaryPosition,
            request.CopyFromTeamId,
            request.IsOnHiatus,
            request.HiatusEndsAt),
            cancellationToken);

        return result.Status switch
        {
            ETeamPlayerOperationStatus.NotFound => NotFound(ToError(StatusCodes.Status404NotFound, "Jogador não encontrado.")),
            ETeamPlayerOperationStatus.InvalidData => BadRequest(ToError(StatusCodes.Status400BadRequest, "Dados inválidos.")),
            _ => Ok(new ApiResponse<PlayerResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Success = true,
                Message = "Perfil atualizado com sucesso.",
                Data = MapToResponse(result.Player!)
            })
        };
    }

    /// <summary>
    /// Atualiza dados administrativos do jogador.
    /// Usado para isenção de mensalidade e ajustes internos.
    /// </summary>
    [HttpPatch("{userId:guid}")]
    [Authorize(Policy = TeamPolicies.TeamAdmin)]
    public async Task<IActionResult> UpdatePlayerAdmin([FromRoute] Guid teamId, [FromRoute] Guid userId, [FromBody] UpdatePlayerAdminRequest request, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var isSystemAdmin = User.IsInRole(SystemRoles.Admin) || User.IsInRole(SystemRoles.Master);

        var result = await _playerHandler.UpdatePlayerAdminAsync(new UpdatePlayerAdminCommand(
            teamId,
            userId,
            currentUserId,
            request.IsFeeExempt,
            isSystemAdmin),
            cancellationToken);

        return result.Status switch
        {
            ETeamPlayerOperationStatus.NotFound => NotFound(ToError(StatusCodes.Status404NotFound, "Jogador não encontrado.")),
            ETeamPlayerOperationStatus.Forbidden => Forbid(),
            ETeamPlayerOperationStatus.InvalidData => BadRequest(ToError(StatusCodes.Status400BadRequest, "Dados inválidos.")),
            _ => Ok(new ApiResponse<PlayerResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Success = true,
                Message = "Perfil atualizado com sucesso.",
                Data = MapToResponse(result.Player!)
            })
        };
    }

    /// <summary>
    /// Remove um jogador do time.
    /// Respeita regras e permissões configuradas.
    /// </summary>
    [HttpDelete("{userId:guid}")]
    [Authorize(Policy = TeamPolicies.TeamModerator)]
    public async Task<IActionResult> RemovePlayer([FromRoute] Guid teamId, [FromRoute] Guid userId, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var isSystemAdmin = User.IsInRole(SystemRoles.Admin) || User.IsInRole(SystemRoles.Master);

        var result = await _playerHandler.RemovePlayerAsync(new RemovePlayerCommand(teamId, userId, currentUserId, isSystemAdmin), cancellationToken);

        return result.Status switch
        {
            ETeamPlayerOperationStatus.NotFound => NotFound(ToError(StatusCodes.Status404NotFound, "Jogador não encontrado.")),
            ETeamPlayerOperationStatus.Forbidden => Forbid(),
            ETeamPlayerOperationStatus.InvalidData => BadRequest(ToError(StatusCodes.Status400BadRequest, "Dados inválidos.")),
            _ => Ok(new ApiResponse<PlayerResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Success = true,
                Message = "Jogador removido com sucesso.",
                Data = MapToResponse(result.Player!)
            })
        };
    }

    private static PlayerResponse MapToResponse(TeamPlayerDto player)
        => new PlayerResponse(
            player.TeamMemberId,
            player.TeamId,
            player.UserId,
            player.FullName,
            player.Role,
            player.Status,
            player.IsFeeExempt,
            player.IsOnHiatus,
            player.HiatusStartedAt,
            player.HiatusEndsAt,
            player.PrimaryPosition,
            player.SecondaryPosition,
            player.TertiaryPosition);

    private static ApiResponse ToError(int statusCode, string message)
        => new ApiResponse
        {
            StatusCode = statusCode,
            Success = false,
            Message = message
        };
}
