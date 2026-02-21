using ConvocadoFc.Domain.Models.Modules.Teams;

namespace ConvocadoFc.Application.Handlers.Modules.Teams.Models;

public sealed record TeamJoinRequestDto(
    Guid Id,
    Guid TeamId,
    Guid UserId,
    Guid? InviteId,
    Guid? ReviewedByUserId,
    ETeamJoinRequestStatus Status,
    ETeamJoinRequestSource Source,
    bool IsAutoApproved,
    string? Message,
    DateTimeOffset RequestedAt,
    DateTimeOffset? ReviewedAt
);
