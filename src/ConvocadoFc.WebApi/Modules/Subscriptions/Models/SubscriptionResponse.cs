using ConvocadoFc.Domain.Models.Modules.Subscriptions;

namespace ConvocadoFc.WebApi.Modules.Subscriptions.Models;

/// <summary>
/// Dados de uma assinatura do usuário.
/// </summary>
/// <param name="Id">Identificador da assinatura.</param>
/// <param name="OwnerUserId">Identificador do usuário dono da assinatura.</param>
/// <param name="PlanId">Identificador do plano.</param>
/// <param name="PlanName">Nome do plano.</param>
/// <param name="PlanCode">Código do plano.</param>
/// <param name="Status">Status atual da assinatura.</param>
/// <param name="StartsAt">Data de início da assinatura.</param>
/// <param name="EndsAt">Data de término da assinatura.</param>
/// <param name="AutoRenew">Indica se a assinatura renova automaticamente.</param>
/// <param name="Capacity">Capacidade do plano aplicado.</param>
public sealed record SubscriptionResponse(
    Guid Id,
    Guid OwnerUserId,
    Guid PlanId,
    string PlanName,
    string PlanCode,
    ESubscriptionStatus Status,
    DateTimeOffset StartsAt,
    DateTimeOffset? EndsAt,
    bool AutoRenew,
    PlanCapacityResponse Capacity
);
