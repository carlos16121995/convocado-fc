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
/// Endpoints de gerenciamento de moderadores do time.
/// Define permissões intermediárias para ações do time.
/// </summary>
[ApiController]
[Route("api/teams/{teamId:guid}/moderators")]
[Authorize(Policy = AuthPolicies.EmailConfirmed)]
[Authorize(Policy = TeamPolicies.TeamAdmin)]
public sealed class ModeratorsController(ITeamAuthorizationHandler authorizationHandler) : ControllerBase
{
    private readonly ITeamAuthorizationHandler _authorizationHandler = authorizationHandler;

    /// <summary>
    /// Lista moderadores do time.
    /// Exibe usuários com papel de moderação.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ListModerators([FromRoute] Guid teamId, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var isSystemAdmin = User.IsInRole(SystemRoles.Admin) || User.IsInRole(SystemRoles.Master);

        var result = await _authorizationHandler.ListModeratorsAsync(teamId, currentUserId, isSystemAdmin, cancellationToken);

        return Ok(new ApiResponse<IReadOnlyCollection<ModeratorResponse>>
        {
            StatusCode = StatusCodes.Status200OK,
            Success = true,
            Message = "Moderadores do time.",
            Data = result.Select(MapToResponse).ToList()
        });
    }

    /// <summary>
    /// Atribui um moderador ao time.
    /// Promove o usuário para papel de moderação.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> AssignModerator([FromRoute] Guid teamId, [FromBody] AssignModeratorRequest request, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var isSystemAdmin = User.IsInRole(SystemRoles.Admin) || User.IsInRole(SystemRoles.Master);

        var result = await _authorizationHandler.AssignModeratorAsync(new AssignModeratorCommand(
            teamId,
            request.UserId,
            currentUserId,
            isSystemAdmin),
            cancellationToken);

        return result.Status switch
        {
            ETeamAuthorizationOperationStatus.NotFound => NotFound(ToError(StatusCodes.Status404NotFound, "Jogador não encontrado.")),
            ETeamAuthorizationOperationStatus.Forbidden => Forbid(),
            ETeamAuthorizationOperationStatus.InvalidData => BadRequest(ToError(StatusCodes.Status400BadRequest, "Dados inválidos.")),
            _ => Ok(new ApiResponse<ModeratorResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Success = true,
                Message = "Moderador atribuído com sucesso.",
                Data = MapToResponse(result.Moderator!)
            })
        };
    }

    /// <summary>
    /// Remove um moderador do time.
    /// Retorna o usuário ao papel padrão.
    /// </summary>
    [HttpDelete("{userId:guid}")]
    public async Task<IActionResult> RemoveModerator([FromRoute] Guid teamId, [FromRoute] Guid userId, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var isSystemAdmin = User.IsInRole(SystemRoles.Admin) || User.IsInRole(SystemRoles.Master);

        var result = await _authorizationHandler.RemoveModeratorAsync(new RemoveModeratorCommand(
            teamId,
            userId,
            currentUserId,
            isSystemAdmin),
            cancellationToken);

        return result.Status switch
        {
            ETeamAuthorizationOperationStatus.NotFound => NotFound(ToError(StatusCodes.Status404NotFound, "Jogador não encontrado.")),
            ETeamAuthorizationOperationStatus.Forbidden => Forbid(),
            ETeamAuthorizationOperationStatus.InvalidData => BadRequest(ToError(StatusCodes.Status400BadRequest, "Dados inválidos.")),
            _ => Ok(new ApiResponse<ModeratorResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Success = true,
                Message = "Moderador removido com sucesso.",
                Data = MapToResponse(result.Moderator!)
            })
        };
    }

    private static ModeratorResponse MapToResponse(TeamModeratorDto moderator)
        => new ModeratorResponse(
            moderator.TeamId,
            moderator.UserId,
            moderator.FullName,
            moderator.Role);

    private static ApiResponse ToError(int statusCode, string message)
        => new ApiResponse
        {
            StatusCode = statusCode,
            Success = false,
            Message = message
        };
}
