using ConvocadoFc.Domain.Models.Modules.Teams;

namespace ConvocadoFc.Application.Handlers.Modules.Teams.Models;

public sealed record TeamPlayerDto(
    Guid TeamMemberId,
    Guid TeamId,
    Guid UserId,
    string FullName,
    ETeamMemberRole Role,
    ETeamMemberStatus Status,
    bool IsFeeExempt,
    bool IsOnHiatus,
    DateTimeOffset? HiatusStartedAt,
    DateTimeOffset? HiatusEndsAt,
    EPlayerPosition? PrimaryPosition,
    EPlayerPosition? SecondaryPosition,
    EPlayerPosition? TertiaryPosition
);
