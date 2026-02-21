namespace ConvocadoFc.Application.Handlers.Modules.Teams.Models;

public sealed record TeamOperationResult(
    ETeamOperationStatus Status,
    TeamDto? Team
);
