namespace ConvocadoFc.Application.Handlers.Modules.Teams.Models;

public sealed record UpdateTeamRuleCommand(
    Guid RuleId,
    Guid UpdatedByUserId,
    string Name,
    string? Description,
    string? Scope,
    string? Target,
    bool IsEnabled,
    DateTimeOffset? StartsAt,
    DateTimeOffset? EndsAt,
    bool IsSystemAdmin
);
