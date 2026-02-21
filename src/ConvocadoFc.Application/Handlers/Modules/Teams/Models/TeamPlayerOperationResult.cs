namespace ConvocadoFc.Application.Handlers.Modules.Teams.Models;

public sealed record TeamPlayerOperationResult(
    ETeamPlayerOperationStatus Status,
    TeamPlayerDto? Player
);
