using ConvocadoFc.Domain.Models.Modules.Subscriptions;

namespace ConvocadoFc.WebApi.Modules.Subscriptions.Models;

/// <summary>
/// Solicitação para alteração de uma assinatura existente.
/// </summary>
/// <param name="PlanId">Novo plano a ser aplicado.</param>
/// <param name="EndsAt">Nova data de término.</param>
/// <param name="AutoRenew">Define a renovação automática.</param>
/// <param name="Status">Novo status da assinatura.</param>
/// <param name="Note">Observações administrativas.</param>
public sealed record ChangeSubscriptionRequest(
    Guid? PlanId,
    DateTimeOffset? EndsAt,
    bool? AutoRenew,
    ESubscriptionStatus? Status,
    string? Note
);
