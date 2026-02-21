namespace ConvocadoFc.Domain.Models.Modules.Subscriptions;

/// <summary>
/// Status possível de uma assinatura.
/// </summary>
public enum ESubscriptionStatus
{
    /// <summary>
    /// Aguardando ativação.
    /// </summary>
    Pending = 0,
    /// <summary>
    /// Assinatura ativa.
    /// </summary>
    Active = 1,
    /// <summary>
    /// Assinatura suspensa.
    /// </summary>
    Suspended = 2,
    /// <summary>
    /// Assinatura cancelada.
    /// </summary>
    Canceled = 3,
    /// <summary>
    /// Assinatura expirada.
    /// </summary>
    Expired = 4
}
