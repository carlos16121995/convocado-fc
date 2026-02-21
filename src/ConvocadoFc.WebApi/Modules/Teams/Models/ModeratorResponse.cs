using ConvocadoFc.Domain.Models.Modules.Teams;

namespace ConvocadoFc.WebApi.Modules.Teams.Models;

/// <summary>
/// Dados do moderador do time.
/// </summary>
/// <param name="TeamId">Identificador do time.</param>
/// <param name="UserId">Identificador do usuário.</param>
/// <param name="FullName">Nome completo do usuário.</param>
/// <param name="Role">Role do usuário no time.</param>
public sealed record ModeratorResponse(
    Guid TeamId,
    Guid UserId,
    string FullName,
    ETeamMemberRole Role
);
