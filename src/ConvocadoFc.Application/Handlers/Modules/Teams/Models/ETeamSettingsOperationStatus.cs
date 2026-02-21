namespace ConvocadoFc.Application.Handlers.Modules.Teams.Models;

/// <summary>
/// Resultado das operações de configuração do time.
/// </summary>
public enum ETeamSettingsOperationStatus
{
    /// <summary>
    /// Operação concluída com sucesso.
    /// </summary>
    Success = 0,
    /// <summary>
    /// Registro não encontrado.
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
