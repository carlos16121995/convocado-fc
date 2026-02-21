namespace ConvocadoFc.Application.Handlers.Modules.Teams.Models;

public sealed record TeamRuleParameterOperationResult(
    ETeamRuleParameterOperationStatus Status,
    TeamRuleParameterDto? Parameter
);
