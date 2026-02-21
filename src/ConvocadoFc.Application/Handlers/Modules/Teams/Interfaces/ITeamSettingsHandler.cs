using ConvocadoFc.Application.Handlers.Modules.Teams.Models;

namespace ConvocadoFc.Application.Handlers.Modules.Teams.Interfaces;

public interface ITeamSettingsHandler
{
    Task<TeamSettingsDto?> GetSettingsAsync(Guid teamId, Guid currentUserId, bool isSystemAdmin, CancellationToken cancellationToken);
    Task<TeamSettingsOperationResult> UpsertSettingsAsync(UpsertTeamSettingsCommand command, CancellationToken cancellationToken);
    Task<TeamRuleOperationResult> CreateRuleAsync(CreateTeamRuleCommand command, CancellationToken cancellationToken);
    Task<TeamRuleOperationResult> UpdateRuleAsync(UpdateTeamRuleCommand command, CancellationToken cancellationToken);
    Task<TeamRuleOperationResult> RemoveRuleAsync(RemoveTeamRuleCommand command, CancellationToken cancellationToken);
    Task<TeamRuleParameterOperationResult> AddRuleParameterAsync(AddTeamRuleParameterCommand command, CancellationToken cancellationToken);
    Task<TeamRuleParameterOperationResult> RemoveRuleParameterAsync(RemoveTeamRuleParameterCommand command, CancellationToken cancellationToken);
}
