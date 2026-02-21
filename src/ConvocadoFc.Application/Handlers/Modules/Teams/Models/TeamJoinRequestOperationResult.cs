namespace ConvocadoFc.Application.Handlers.Modules.Teams.Models;

public sealed record TeamJoinRequestOperationResult(
    ETeamJoinRequestOperationStatus Status,
    TeamJoinRequestDto? Request
);
