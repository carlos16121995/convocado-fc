namespace ConvocadoFc.Domain.Models.Modules.Teams;

/// <summary>
/// Status do vínculo do jogador com o time.
/// </summary>
public enum ETeamMemberStatus
{
	/// <summary>
	/// Aguardando aprovação.
	/// </summary>
	Pending = 0,
	/// <summary>
	/// Participação ativa.
	/// </summary>
	Active = 1,
	/// <summary>
	/// Removido do time.
	/// </summary>
	Removed = 2,
	/// <summary>
	/// Banido do time.
	/// </summary>
	Banned = 3
}
