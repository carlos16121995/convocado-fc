namespace ConvocadoFc.WebApi.Modules.Subscriptions.Models;

/// <summary>
/// Dados de um plano de assinatura.
/// </summary>
/// <param name="Id">Identificador do plano.</param>
/// <param name="Name">Nome do plano.</param>
/// <param name="Code">Código único do plano.</param>
/// <param name="Price">Preço do plano.</param>
/// <param name="Currency">Moeda do preço.</param>
/// <param name="IsActive">Indica se o plano está ativo.</param>
/// <param name="IsCustomPricing">Indica se o plano possui preço customizado.</param>
/// <param name="Capacity">Capacidades do plano.</param>
public sealed record PlanResponse(
    Guid Id,
    string Name,
    string Code,
    decimal? Price,
    string Currency,
    bool IsActive,
    bool IsCustomPricing,
    PlanCapacityResponse Capacity
);
