namespace ConvocadoFc.Application.Handlers.Modules.Teams.Models;

public sealed record TeamInviteOperationResult(
    ETeamInviteOperationStatus Status,
    TeamInviteDto? Invite
);
