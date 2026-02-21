namespace ConvocadoFc.Application.Handlers.Modules.Teams.Models;

/// <summary>
/// Resultado das operações de solicitação de entrada em time.
/// </summary>
public enum ETeamJoinRequestOperationStatus
{
    /// <summary>
    /// Operação concluída com sucesso.
    /// </summary>
    Success = 0,
    /// <summary>
    /// Solicitação não encontrada.
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
    /// Solicitação já processada.
    /// </summary>
    AlreadyProcessed = 5,
    /// <summary>
    /// Time não encontrado.
    /// </summary>
    TeamNotFound = 6,
    /// <summary>
    /// Usuário não encontrado.
    /// </summary>
    UserNotFound = 7,
    /// <summary>
    /// Convite não encontrado.
    /// </summary>
    InviteNotFound = 8,
    /// <summary>
    /// Convite atingiu o limite de usos.
    /// </summary>
    MaxUsesReached = 9,
    /// <summary>
    /// Convite expirado.
    /// </summary>
    InviteExpired = 10
}
