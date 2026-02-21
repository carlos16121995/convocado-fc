using ConvocadoFc.Domain.Models.Modules.Teams;

namespace ConvocadoFc.Application.Handlers.Modules.Teams.Models;

public sealed record TeamModeratorDto(
    Guid TeamId,
    Guid UserId,
    string FullName,
    ETeamMemberRole Role
);
