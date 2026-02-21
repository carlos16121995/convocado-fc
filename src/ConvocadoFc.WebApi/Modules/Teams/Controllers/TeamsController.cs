using ConvocadoFc.Application.Handlers.Modules.Teams.Interfaces;
using ConvocadoFc.Application.Handlers.Modules.Teams.Models;
using ConvocadoFc.Domain.Models.Modules.Users.Identity;
using ConvocadoFc.Domain.Shared;
using ConvocadoFc.WebApi.Authorization;
using ConvocadoFc.WebApi.Extensions;
using ConvocadoFc.WebApi.Modules.Teams.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConvocadoFc.WebApi.Modules.Teams.Controllers;

/// <summary>
/// Endpoints de gerenciamento de times e seus dados básicos.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthPolicies.EmailConfirmed)]
public sealed class TeamsController(ITeamManagementHandler teamHandler) : ControllerBase
{
    private readonly ITeamManagementHandler _teamHandler = teamHandler;

    /// <summary>
    /// Lista times vinculados ao usuário ou ao proprietário informado.
    /// Suporta paginação e ordenação.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ListTeams([FromQuery] ListTeamsQueryModel query, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var result = await _teamHandler.ListTeamsAsync(new ListTeamsQuery(
            new PaginationQuery
            {
                Page = query.Page,
                PageSize = query.PageSize,
                OrderBy = query.OrderBy
            },
            query.OwnerUserId ?? currentUserId),
            cancellationToken);

        return Ok(new ApiResponse<PaginatedResult<TeamResponse>>
        {
            StatusCode = StatusCodes.Status200OK,
            Success = true,
            Message = "Times encontrados.",
            Data = new PaginatedResult<TeamResponse>
            {
                TotalItems = result.TotalItems,
                Items = result.Items.Select(MapToResponse).ToList()
            }
        });
    }

    /// <summary>
    /// Obtém um time por id.
    /// Retorna dados principais e status.
    /// </summary>
    [HttpGet("{teamId:guid}")]
    public async Task<IActionResult> GetTeam([FromRoute] Guid teamId, CancellationToken cancellationToken)
    {
        var team = await _teamHandler.GetTeamAsync(teamId, cancellationToken);
        if (team is null)
        {
            return NotFound(ToError(StatusCodes.Status404NotFound, "Time não encontrado."));
        }

        return Ok(new ApiResponse<TeamResponse>
        {
            StatusCode = StatusCodes.Status200OK,
            Success = true,
            Message = "Time encontrado.",
            Data = MapToResponse(team)
        });
    }

    /// <summary>
    /// Cria um novo time.
    /// Requer permissões administrativas globais.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = SystemRoles.AdminOrMaster)]
    public async Task<IActionResult> CreateTeam([FromBody] CreateTeamRequest request, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var isSystemAdmin = User.IsInRole(SystemRoles.Admin) || User.IsInRole(SystemRoles.Master);

        var result = await _teamHandler.CreateTeamAsync(new CreateTeamCommand(
            currentUserId,
            request.Name,
            request.HomeFieldName,
            request.HomeFieldAddress,
            request.HomeFieldLatitude,
            request.HomeFieldLongitude,
            request.CrestUrl,
            isSystemAdmin),
            cancellationToken);

        return result.Status switch
        {
            ETeamOperationStatus.UserNotFound => NotFound(ToError(StatusCodes.Status404NotFound, "Usuário não encontrado.")),
            ETeamOperationStatus.NameAlreadyExists => Conflict(ToError(StatusCodes.Status409Conflict, "Já existe um time com esse nome.")),
            ETeamOperationStatus.InvalidData => BadRequest(ToError(StatusCodes.Status400BadRequest, "Dados inválidos.")),
            ETeamOperationStatus.Forbidden => Forbid(),
            _ => Ok(new ApiResponse<TeamResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Success = true,
                Message = "Time criado com sucesso.",
                Data = MapToResponse(result.Team!)
            })
        };
    }

    /// <summary>
    /// Atualiza um time.
    /// Respeita a política de permissões do time.
    /// </summary>
    [HttpPut("{teamId:guid}")]
    [Authorize(Policy = TeamPolicies.TeamAdmin)]
    public async Task<IActionResult> UpdateTeam([FromRoute] Guid teamId, [FromBody] UpdateTeamRequest request, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var isSystemAdmin = User.IsInRole(SystemRoles.Admin) || User.IsInRole(SystemRoles.Master);

        if (teamId != request.TeamId)
        {
            return BadRequest(ToError(StatusCodes.Status400BadRequest, "Time inválido."));
        }

        var result = await _teamHandler.UpdateTeamAsync(new UpdateTeamCommand(
            request.TeamId,
            currentUserId,
            request.Name,
            request.HomeFieldName,
            request.HomeFieldAddress,
            request.HomeFieldLatitude,
            request.HomeFieldLongitude,
            request.CrestUrl,
            request.IsActive,
            isSystemAdmin),
            cancellationToken);

        return result.Status switch
        {
            ETeamOperationStatus.NotFound => NotFound(ToError(StatusCodes.Status404NotFound, "Time não encontrado.")),
            ETeamOperationStatus.NameAlreadyExists => Conflict(ToError(StatusCodes.Status409Conflict, "Já existe um time com esse nome.")),
            ETeamOperationStatus.Forbidden => Forbid(),
            ETeamOperationStatus.InvalidData => BadRequest(ToError(StatusCodes.Status400BadRequest, "Dados inválidos.")),
            _ => Ok(new ApiResponse<TeamResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Success = true,
                Message = "Time atualizado com sucesso.",
                Data = MapToResponse(result.Team!)
            })
        };
    }

    /// <summary>
    /// Remove um time.
    /// Exige autorização administrativa no time.
    /// </summary>
    [HttpDelete("{teamId:guid}")]
    [Authorize(Policy = TeamPolicies.TeamAdmin)]
    public async Task<IActionResult> RemoveTeam([FromRoute] Guid teamId, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var isSystemAdmin = User.IsInRole(SystemRoles.Admin) || User.IsInRole(SystemRoles.Master);

        var result = await _teamHandler.RemoveTeamAsync(teamId, currentUserId, isSystemAdmin, cancellationToken);

        return result.Status switch
        {
            ETeamOperationStatus.NotFound => NotFound(ToError(StatusCodes.Status404NotFound, "Time não encontrado.")),
            ETeamOperationStatus.Forbidden => Forbid(),
            _ => Ok(new ApiResponse<TeamResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Success = true,
                Message = "Time removido com sucesso.",
                Data = MapToResponse(result.Team!)
            })
        };
    }

    private static TeamResponse MapToResponse(TeamDto team)
        => new TeamResponse(
            team.Id,
            team.OwnerUserId,
            team.Name,
            team.HomeFieldName,
            team.HomeFieldAddress,
            team.HomeFieldLatitude,
            team.HomeFieldLongitude,
            team.CrestUrl,
            team.IsActive,
            team.CreatedAt,
            team.UpdatedAt);

    private static ApiResponse ToError(int statusCode, string message)
        => new ApiResponse
        {
            StatusCode = statusCode,
            Success = false,
            Message = message
        };
}
