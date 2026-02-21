namespace ConvocadoFc.Domain.Models.Modules.Subscriptions;

/// <summary>
/// Ações registradas no histórico da assinatura.
/// </summary>
public enum ESubscriptionHistoryAction
{
    /// <summary>
    /// Assinatura atribuída ao usuário.
    /// </summary>
    Assigned = 0,
    /// <summary>
    /// Assinatura alterada.
    /// </summary>
    Changed = 1,
    /// <summary>
    /// Assinatura removida.
    /// </summary>
    Removed = 2
}
