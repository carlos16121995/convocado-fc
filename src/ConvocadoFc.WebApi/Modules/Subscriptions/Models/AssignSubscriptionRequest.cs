namespace ConvocadoFc.WebApi.Modules.Subscriptions.Models;

/// <summary>
/// Solicitação para atribuir uma assinatura a um usuário.
/// </summary>
/// <param name="UserId">Identificador do usuário.</param>
/// <param name="PlanId">Identificador do plano.</param>
/// <param name="StartsAt">Data de início da assinatura.</param>
/// <param name="EndsAt">Data de término da assinatura.</param>
/// <param name="AutoRenew">Indica se a assinatura renova automaticamente.</param>
/// <param name="Note">Observações administrativas.</param>
public sealed record AssignSubscriptionRequest(
    Guid UserId,
    Guid PlanId,
    DateTimeOffset? StartsAt,
    DateTimeOffset? EndsAt,
    bool AutoRenew,
    string? Note
);
