namespace ConvocadoFc.WebApi.Modules.Subscriptions.Models;

/// <summary>
/// Solicitação para remover uma assinatura.
/// </summary>
/// <param name="Note">Observações administrativas.</param>
public sealed record RemoveSubscriptionRequest(
    string? Note
);
