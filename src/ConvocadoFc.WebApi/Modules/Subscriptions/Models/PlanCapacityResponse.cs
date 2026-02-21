namespace ConvocadoFc.WebApi.Modules.Subscriptions.Models;

/// <summary>
/// Limites de capacidade de um plano.
/// </summary>
/// <param name="MaxTeams">Quantidade máxima de times permitida.</param>
/// <param name="MaxMembersPerTeam">Quantidade máxima de membros por time.</param>
public sealed record PlanCapacityResponse(
    int? MaxTeams,
    int? MaxMembersPerTeam
);
