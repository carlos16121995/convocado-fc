namespace ConvocadoFc.Application.Handlers.Modules.Teams.Models;

public sealed record TeamAuthorizationOperationResult(
    ETeamAuthorizationOperationStatus Status,
    TeamModeratorDto? Moderator
);
