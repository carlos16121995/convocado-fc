using ConvocadoFc.Domain.Models.Modules.Teams;

namespace ConvocadoFc.Application.Handlers.Modules.Teams.Models;

public sealed record CreateJoinRequestCommand(
    Guid TeamId,
    Guid UserId,
    string? Message,
    Guid? InviteId,
    ETeamJoinRequestSource Source
);
