namespace ConvocadoFc.Domain.Models.Modules.Teams;

/// <summary>
/// Status do convite para o time.
/// </summary>
public enum ETeamInviteStatus
{
	/// <summary>
	/// Aguardando resposta.
	/// </summary>
	Pending = 0,
	/// <summary>
	/// Convite aceito.
	/// </summary>
	Accepted = 1,
	/// <summary>
	/// Convite recusado.
	/// </summary>
	Declined = 2,
	/// <summary>
	/// Convite expirado.
	/// </summary>
	Expired = 3,
	/// <summary>
	/// Convite revogado.
	/// </summary>
	Revoked = 4
}
