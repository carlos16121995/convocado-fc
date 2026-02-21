namespace ConvocadoFc.Application.Handlers.Modules.Subscriptions.Models;

/// <summary>
/// Resultado das operações de plano de assinatura.
/// </summary>
public enum EPlanOperationStatus
{
    /// <summary>
    /// Operação concluída com sucesso.
    /// </summary>
    Success = 0,
    /// <summary>
    /// Plano não encontrado.
    /// </summary>
    NotFound = 1,
    /// <summary>
    /// Código de plano já existe.
    /// </summary>
    CodeAlreadyExists = 2,
    /// <summary>
    /// Nome de plano já existe.
    /// </summary>
    NameAlreadyExists = 3,
    /// <summary>
    /// Capacidade inválida.
    /// </summary>
    InvalidCapacity = 4
}
