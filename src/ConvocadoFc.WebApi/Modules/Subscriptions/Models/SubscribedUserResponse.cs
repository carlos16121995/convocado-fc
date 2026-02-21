namespace ConvocadoFc.WebApi.Modules.Subscriptions.Models;

/// <summary>
/// Dados do usuário com assinatura.
/// </summary>
/// <param name="UserId">Identificador do usuário.</param>
/// <param name="Email">E-mail do usuário.</param>
/// <param name="FullName">Nome completo do usuário.</param>
/// <param name="Subscription">Dados da assinatura do usuário.</param>
public sealed record SubscribedUserResponse(
    Guid UserId,
    string Email,
    string FullName,
    SubscriptionResponse Subscription
);
