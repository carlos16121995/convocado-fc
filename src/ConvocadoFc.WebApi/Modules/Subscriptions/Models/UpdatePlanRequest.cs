namespace ConvocadoFc.WebApi.Modules.Subscriptions.Models;

/// <summary>
/// Solicitação para atualização de um plano.
/// </summary>
/// <param name="Name">Nome do plano.</param>
/// <param name="Code">Código único do plano.</param>
/// <param name="Price">Preço do plano.</param>
/// <param name="Currency">Moeda do preço.</param>
/// <param name="MaxTeams">Quantidade máxima de times.</param>
/// <param name="MaxMembersPerTeam">Quantidade máxima de membros por time.</param>
/// <param name="IsCustomPricing">Indica se o plano possui preço customizado.</param>
/// <param name="IsActive">Indica se o plano está ativo.</param>
public sealed record UpdatePlanRequest(
    string Name,
    string Code,
    decimal? Price,
    string Currency,
    int? MaxTeams,
    int? MaxMembersPerTeam,
    bool IsCustomPricing,
    bool IsActive
);
