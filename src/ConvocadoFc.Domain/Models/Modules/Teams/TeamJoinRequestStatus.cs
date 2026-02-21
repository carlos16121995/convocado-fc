namespace ConvocadoFc.Domain.Models.Modules.Teams;

/// <summary>
/// Status da solicitação de entrada no time.
/// </summary>
public enum ETeamJoinRequestStatus
{
	/// <summary>
	/// Aguardando revisão.
	/// </summary>
	Pending = 0,
	/// <summary>
	/// Solicitação aprovada.
	/// </summary>
	Approved = 1,
	/// <summary>
	/// Solicitação rejeitada.
	/// </summary>
	Rejected = 2,
	/// <summary>
	/// Solicitação cancelada.
	/// </summary>
	Cancelled = 3
}
