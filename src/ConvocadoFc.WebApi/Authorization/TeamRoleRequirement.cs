using ConvocadoFc.Domain.Models.Modules.Teams;

using Microsoft.AspNetCore.Authorization;

namespace ConvocadoFc.WebApi.Authorization;

public sealed class TeamRoleRequirement(IReadOnlyCollection<ETeamMemberRole> allowedRoles) : IAuthorizationRequirement
{
    public IReadOnlyCollection<ETeamMemberRole> AllowedRoles { get; } = allowedRoles;
}
