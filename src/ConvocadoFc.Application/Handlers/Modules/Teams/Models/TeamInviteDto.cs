using ConvocadoFc.Domain.Models.Modules.Teams;

namespace ConvocadoFc.Application.Handlers.Modules.Teams.Models;

public sealed record TeamInviteDto(
    Guid Id,
    Guid TeamId,
    Guid CreatedByUserId,
    Guid? TargetUserId,
    string? TargetEmail,
    string Token,
    ETeamInviteChannel Channel,
    ETeamInviteStatus Status,
    bool IsPreApproved,
    int? MaxUses,
    int UseCount,
    string? Message,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ExpiresAt,
    DateTimeOffset? AcceptedAt
);
