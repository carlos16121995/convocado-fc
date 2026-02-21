using ConvocadoFc.Domain.Models.Modules.Teams;

namespace ConvocadoFc.Application.Handlers.Modules.Teams.Models;

public sealed record UpdateMyProfileCommand(
    Guid TeamId,
    Guid UserId,
    EPlayerPosition? PrimaryPosition,
    EPlayerPosition? SecondaryPosition,
    EPlayerPosition? TertiaryPosition,
    Guid? CopyFromTeamId,
    bool? IsOnHiatus,
    DateTimeOffset? HiatusEndsAt
);
