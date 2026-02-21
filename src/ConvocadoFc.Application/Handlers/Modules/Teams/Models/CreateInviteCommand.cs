using ConvocadoFc.Domain.Models.Modules.Teams;

namespace ConvocadoFc.Application.Handlers.Modules.Teams.Models;

public sealed record CreateInviteCommand(
    Guid TeamId,
    Guid CreatedByUserId,
    Guid? TargetUserId,
    string? TargetEmail,
    ETeamInviteChannel Channel,
    int? MaxUses,
    DateTimeOffset? ExpiresAt,
    string? Message,
    bool IsSystemAdmin
);
