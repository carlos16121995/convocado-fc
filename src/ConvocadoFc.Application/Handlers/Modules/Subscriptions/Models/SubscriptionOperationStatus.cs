namespace ConvocadoFc.Application.Handlers.Modules.Subscriptions.Models;

/// <summary>
/// Resultado das operações de assinatura.
/// </summary>
public enum ESubscriptionOperationStatus
{
    /// <summary>
    /// Operação concluída com sucesso.
    /// </summary>
    Success = 0,
    /// <summary>
    /// Assinatura não encontrada.
    /// </summary>
    NotFound = 1,
    /// <summary>
    /// Plano não encontrado.
    /// </summary>
    PlanNotFound = 2,
    /// <summary>
    /// Usuário não encontrado.
    /// </summary>
    UserNotFound = 3,
    /// <summary>
    /// Usuário já possui assinatura ativa.
    /// </summary>
    ActiveSubscriptionExists = 4,
    /// <summary>
    /// Assinatura não está ativa.
    /// </summary>
    SubscriptionNotActive = 5
}
