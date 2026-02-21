namespace ConvocadoFc.Application.Handlers.Modules.Teams.Models;

/// <summary>
/// Resultado das operações de gerenciamento de times.
/// </summary>
public enum ETeamOperationStatus
{
    /// <summary>
    /// Operação concluída com sucesso.
    /// </summary>
    Success = 0,
    /// <summary>
    /// Time não encontrado.
    /// </summary>
    NotFound = 1,
    /// <summary>
    /// Usuário não encontrado.
    /// </summary>
    UserNotFound = 2,
    /// <summary>
    /// Nome de time já existente.
    /// </summary>
    NameAlreadyExists = 3,
    /// <summary>
    /// Operação não permitida.
    /// </summary>
    Forbidden = 4,
    /// <summary>
    /// Dados inválidos.
    /// </summary>
    InvalidData = 5
}
