namespace ConvocadoFc.Application.Handlers.Modules.Teams.Models;

public sealed record AcceptInviteCommand(
    Guid InviteId,
    Guid UserId
);
