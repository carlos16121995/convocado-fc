using ConvocadoFc.Domain.Models.Modules.Subscriptions;

namespace ConvocadoFc.WebApi.Modules.Subscriptions.Models;

/// <summary>
/// Filtros para listagem de assinaturas.
/// </summary>
public sealed record ListSubscriptionsQueryModel
{
    /// <summary>
    /// Filtra por usuário.
    /// </summary>
    public Guid? UserId { get; init; }

    /// <summary>
    /// Filtra pelo status da assinatura.
    /// </summary>
    public ESubscriptionStatus? Status { get; init; }

    /// <summary>
    /// Filtra pelo plano.
    /// </summary>
    public Guid? PlanId { get; init; }
}
