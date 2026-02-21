namespace ConvocadoFc.Application.Handlers.Modules.Teams.Models;

public sealed record CreateTeamRuleCommand(
    Guid TeamId,
    Guid CreatedByUserId,
    string Code,
    string Name,
    string? Description,
    string? Scope,
    string? Target,
    bool IsEnabled,
    DateTimeOffset? StartsAt,
    DateTimeOffset? EndsAt,
    bool IsSystemAdmin
);
