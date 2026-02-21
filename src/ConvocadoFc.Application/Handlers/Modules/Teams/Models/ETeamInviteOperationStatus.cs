namespace ConvocadoFc.Application.Handlers.Modules.Teams.Models;

/// <summary>
/// Resultado das operações de convites do time.
/// </summary>
public enum ETeamInviteOperationStatus
{
    /// <summary>
    /// Operação concluída com sucesso.
    /// </summary>
    Success = 0,
    /// <summary>
    /// Convite não encontrado.
    /// </summary>
    NotFound = 1,
    /// <summary>
    /// Operação não permitida.
    /// </summary>
    Forbidden = 2,
    /// <summary>
    /// Dados inválidos.
    /// </summary>
    InvalidData = 3,
    /// <summary>
    /// Usuário já é membro do time.
    /// </summary>
    AlreadyMember = 4,
    /// <summary>
    /// Convite já processado.
    /// </summary>
    AlreadyProcessed = 5,
    /// <summary>
    /// Convite expirado.
    /// </summary>
    InviteExpired = 6,
    /// <summary>
    /// Convite atingiu o limite de usos.
    /// </summary>
    MaxUsesReached = 7,
    /// <summary>
    /// Time não encontrado.
    /// </summary>
    TeamNotFound = 8,
    /// <summary>
    /// Usuário não encontrado.
    /// </summary>
    UserNotFound = 9
}
