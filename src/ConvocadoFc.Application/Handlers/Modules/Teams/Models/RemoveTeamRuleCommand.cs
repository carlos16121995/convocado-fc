namespace ConvocadoFc.Application.Handlers.Modules.Teams.Models;

public sealed record RemoveTeamRuleCommand(
    Guid RuleId,
    Guid RemovedByUserId,
    bool IsSystemAdmin
);
