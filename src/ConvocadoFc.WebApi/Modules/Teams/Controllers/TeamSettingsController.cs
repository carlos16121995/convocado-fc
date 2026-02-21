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
/// Endpoints de configurações e regras do time.
/// Controla políticas e parâmetros de funcionamento.
/// </summary>
[ApiController]
[Route("api/teams/{teamId:guid}/settings")]
[Authorize(Policy = AuthPolicies.EmailConfirmed)]
[Authorize(Policy = TeamPolicies.TeamAdmin)]
public sealed class TeamSettingsController(ITeamSettingsHandler settingsHandler) : ControllerBase
{
    private readonly ITeamSettingsHandler _settingsHandler = settingsHandler;

    /// <summary>
    /// Obtém as configurações do time.
    /// Retorna parâmetros e regras ativas.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetSettings([FromRoute] Guid teamId, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var isSystemAdmin = User.IsInRole(SystemRoles.Admin) || User.IsInRole(SystemRoles.Master);

        var settings = await _settingsHandler.GetSettingsAsync(teamId, currentUserId, isSystemAdmin, cancellationToken);
        if (settings is null)
        {
            return Forbid();
        }

        return Ok(new ApiResponse<TeamSettingsResponse>
        {
            StatusCode = StatusCodes.Status200OK,
            Success = true,
            Message = "Configurações do time.",
            Data = MapToResponse(settings)
        });
    }

    /// <summary>
    /// Cria ou atualiza as configurações do time.
    /// Persiste lista completa de chaves e valores.
    /// </summary>
    [HttpPut]
    public async Task<IActionResult> UpsertSettings([FromRoute] Guid teamId, [FromBody] UpsertTeamSettingsRequest request, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var isSystemAdmin = User.IsInRole(SystemRoles.Admin) || User.IsInRole(SystemRoles.Master);

        var result = await _settingsHandler.UpsertSettingsAsync(new UpsertTeamSettingsCommand(
            teamId,
            currentUserId,
            request.Settings.Select(item => new UpsertTeamSettingEntry(
                item.Key,
                item.Value,
                item.ValueType,
                item.IsEnabled,
                item.Description)).ToList(),
            isSystemAdmin),
            cancellationToken);

        return result.Status switch
        {
            ETeamSettingsOperationStatus.Forbidden => Forbid(),
            ETeamSettingsOperationStatus.InvalidData => BadRequest(ToError(StatusCodes.Status400BadRequest, "Dados inválidos.")),
            _ => Ok(new ApiResponse<TeamSettingsResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Success = true,
                Message = "Configurações atualizadas.",
                Data = MapToResponse(result.Settings!)
            })
        };
    }

    /// <summary>
    /// Cria uma regra para o time.
    /// Define escopo, alvo e período de vigência.
    /// </summary>
    [HttpPost("rules")]
    public async Task<IActionResult> CreateRule([FromRoute] Guid teamId, [FromBody] CreateTeamRuleRequest request, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var isSystemAdmin = User.IsInRole(SystemRoles.Admin) || User.IsInRole(SystemRoles.Master);

        var result = await _settingsHandler.CreateRuleAsync(new CreateTeamRuleCommand(
            teamId,
            currentUserId,
            request.Code,
            request.Name,
            request.Description,
            request.Scope,
            request.Target,
            request.IsEnabled,
            request.StartsAt,
            request.EndsAt,
            isSystemAdmin),
            cancellationToken);

        return result.Status switch
        {
            ETeamRuleOperationStatus.Forbidden => Forbid(),
            ETeamRuleOperationStatus.InvalidData => BadRequest(ToError(StatusCodes.Status400BadRequest, "Dados inválidos.")),
            _ => Ok(new ApiResponse<TeamRuleResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Success = true,
                Message = "Regra criada.",
                Data = MapToResponse(result.Rule!)
            })
        };
    }

    /// <summary>
    /// Atualiza uma regra do time.
    /// Permite habilitar, descrever e ajustar vigência.
    /// </summary>
    [HttpPut("rules/{ruleId:guid}")]
    public async Task<IActionResult> UpdateRule([FromRoute] Guid ruleId, [FromBody] UpdateTeamRuleRequest request, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var isSystemAdmin = User.IsInRole(SystemRoles.Admin) || User.IsInRole(SystemRoles.Master);

        var result = await _settingsHandler.UpdateRuleAsync(new UpdateTeamRuleCommand(
            ruleId,
            currentUserId,
            request.Name,
            request.Description,
            request.Scope,
            request.Target,
            request.IsEnabled,
            request.StartsAt,
            request.EndsAt,
            isSystemAdmin),
            cancellationToken);

        return result.Status switch
        {
            ETeamRuleOperationStatus.NotFound => NotFound(ToError(StatusCodes.Status404NotFound, "Regra não encontrada.")),
            ETeamRuleOperationStatus.Forbidden => Forbid(),
            ETeamRuleOperationStatus.InvalidData => BadRequest(ToError(StatusCodes.Status400BadRequest, "Dados inválidos.")),
            _ => Ok(new ApiResponse<TeamRuleResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Success = true,
                Message = "Regra atualizada.",
                Data = MapToResponse(result.Rule!)
            })
        };
    }

    /// <summary>
    /// Remove uma regra do time.
    /// Exclui a regra e seus parâmetros associados.
    /// </summary>
    [HttpDelete("rules/{ruleId:guid}")]
    public async Task<IActionResult> RemoveRule([FromRoute] Guid ruleId, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var isSystemAdmin = User.IsInRole(SystemRoles.Admin) || User.IsInRole(SystemRoles.Master);

        var result = await _settingsHandler.RemoveRuleAsync(new RemoveTeamRuleCommand(ruleId, currentUserId, isSystemAdmin), cancellationToken);

        return result.Status switch
        {
            ETeamRuleOperationStatus.NotFound => NotFound(ToError(StatusCodes.Status404NotFound, "Regra não encontrada.")),
            ETeamRuleOperationStatus.Forbidden => Forbid(),
            ETeamRuleOperationStatus.InvalidData => BadRequest(ToError(StatusCodes.Status400BadRequest, "Dados inválidos.")),
            _ => Ok(new ApiResponse
            {
                StatusCode = StatusCodes.Status200OK,
                Success = true,
                Message = "Regra removida."
            })
        };
    }

    /// <summary>
    /// Adiciona um parâmetro a uma regra.
    /// Usado para valores configuráveis e limites.
    /// </summary>
    [HttpPost("rules/{ruleId:guid}/parameters")]
    public async Task<IActionResult> AddRuleParameter([FromRoute] Guid ruleId, [FromBody] AddTeamRuleParameterRequest request, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var isSystemAdmin = User.IsInRole(SystemRoles.Admin) || User.IsInRole(SystemRoles.Master);

        var result = await _settingsHandler.AddRuleParameterAsync(new AddTeamRuleParameterCommand(
            ruleId,
            currentUserId,
            request.Key,
            request.Value,
            request.ValueType,
            request.Unit,
            request.Description,
            isSystemAdmin),
            cancellationToken);

        return result.Status switch
        {
            ETeamRuleParameterOperationStatus.NotFound => NotFound(ToError(StatusCodes.Status404NotFound, "Regra não encontrada.")),
            ETeamRuleParameterOperationStatus.Forbidden => Forbid(),
            ETeamRuleParameterOperationStatus.InvalidData => BadRequest(ToError(StatusCodes.Status400BadRequest, "Dados inválidos.")),
            _ => Ok(new ApiResponse<TeamRuleParameterResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Success = true,
                Message = "Parâmetro adicionado.",
                Data = MapToResponse(result.Parameter!)
            })
        };
    }

    /// <summary>
    /// Remove um parâmetro de uma regra.
    /// Atualiza a regra com os parâmetros restantes.
    /// </summary>
    [HttpDelete("rules/{ruleId:guid}/parameters/{parameterId:guid}")]
    public async Task<IActionResult> RemoveRuleParameter([FromRoute] Guid ruleId, [FromRoute] Guid parameterId, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var isSystemAdmin = User.IsInRole(SystemRoles.Admin) || User.IsInRole(SystemRoles.Master);

        var result = await _settingsHandler.RemoveRuleParameterAsync(new RemoveTeamRuleParameterCommand(parameterId, currentUserId, isSystemAdmin), cancellationToken);

        return result.Status switch
        {
            ETeamRuleParameterOperationStatus.NotFound => NotFound(ToError(StatusCodes.Status404NotFound, "Parâmetro não encontrado.")),
            ETeamRuleParameterOperationStatus.Forbidden => Forbid(),
            ETeamRuleParameterOperationStatus.InvalidData => BadRequest(ToError(StatusCodes.Status400BadRequest, "Dados inválidos.")),
            _ => Ok(new ApiResponse
            {
                StatusCode = StatusCodes.Status200OK,
                Success = true,
                Message = "Parâmetro removido."
            })
        };
    }

    private static TeamSettingsResponse MapToResponse(TeamSettingsDto settings)
        => new TeamSettingsResponse(
            settings.TeamId,
            settings.Settings.Select(MapToResponse).ToList(),
            settings.Rules.Select(MapToResponse).ToList());

    private static TeamSettingEntryResponse MapToResponse(TeamSettingEntryDto entry)
        => new TeamSettingEntryResponse(
            entry.Id,
            entry.Key,
            entry.Value,
            entry.ValueType,
            entry.IsEnabled,
            entry.Description);

    private static TeamRuleResponse MapToResponse(TeamRuleDto rule)
        => new TeamRuleResponse(
            rule.Id,
            rule.Code,
            rule.Name,
            rule.Description,
            rule.Scope,
            rule.Target,
            rule.IsEnabled,
            rule.StartsAt,
            rule.EndsAt,
            rule.Parameters.Select(MapToResponse).ToList());

    private static TeamRuleParameterResponse MapToResponse(TeamRuleParameterDto parameter)
        => new TeamRuleParameterResponse(
            parameter.Id,
            parameter.Key,
            parameter.Value,
            parameter.ValueType,
            parameter.Unit,
            parameter.Description);

    private static ApiResponse ToError(int statusCode, string message)
        => new ApiResponse
        {
            StatusCode = statusCode,
            Success = false,
            Message = message
        };
}
