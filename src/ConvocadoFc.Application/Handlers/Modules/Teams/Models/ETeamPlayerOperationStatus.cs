namespace ConvocadoFc.Application.Handlers.Modules.Teams.Models;

/// <summary>
/// Resultado das operações sobre jogadores do time.
/// </summary>
public enum ETeamPlayerOperationStatus
{
    /// <summary>
    /// Operação concluída com sucesso.
    /// </summary>
    Success = 0,
    /// <summary>
    /// Jogador não encontrado.
    /// </summary>
    NotFound = 1,
    /// <summary>
    /// Operação não permitida.
    /// </summary>
    Forbidden = 2,
    /// <summary>
    /// Dados inválidos.
    /// </summary>
    InvalidData = 3
}
