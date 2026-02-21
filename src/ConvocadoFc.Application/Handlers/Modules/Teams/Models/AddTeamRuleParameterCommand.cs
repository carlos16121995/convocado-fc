namespace ConvocadoFc.Application.Handlers.Modules.Teams.Models;

public sealed record AddTeamRuleParameterCommand(
    Guid RuleId,
    Guid AddedByUserId,
    string Key,
    string Value,
    string? ValueType,
    string? Unit,
    string? Description,
    bool IsSystemAdmin
);
