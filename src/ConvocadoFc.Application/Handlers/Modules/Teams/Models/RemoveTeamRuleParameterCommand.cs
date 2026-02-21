namespace ConvocadoFc.Application.Handlers.Modules.Teams.Models;

public sealed record RemoveTeamRuleParameterCommand(
    Guid RuleParameterId,
    Guid RemovedByUserId,
    bool IsSystemAdmin
);
