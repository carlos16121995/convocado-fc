namespace ConvocadoFc.Application.Handlers.Modules.Teams.Models;

/// <summary>
/// Resultado das operações sobre parâmetros de regras.
/// </summary>
public enum ETeamRuleParameterOperationStatus
{
    /// <summary>
    /// Operação concluída com sucesso.
    /// </summary>
    Success = 0,
    /// <summary>
    /// Parâmetro não encontrado.
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
