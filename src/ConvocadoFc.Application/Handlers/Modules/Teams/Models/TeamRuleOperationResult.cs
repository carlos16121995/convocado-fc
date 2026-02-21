namespace ConvocadoFc.Application.Handlers.Modules.Teams.Models;

public sealed record TeamRuleOperationResult(
    ETeamRuleOperationStatus Status,
    TeamRuleDto? Rule
);
