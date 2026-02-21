namespace ConvocadoFc.Domain.Models.Modules.Teams;

/// <summary>
/// Origem da solicitação de entrada no time.
/// </summary>
public enum ETeamJoinRequestSource
{
	/// <summary>
	/// Busca por proximidade.
	/// </summary>
	ProximitySearch = 0,
	/// <summary>
	/// Link compartilhável.
	/// </summary>
	ShareLink = 1,
	/// <summary>
	/// QR Code.
	/// </summary>
	QrCode = 2,
	/// <summary>
	/// Convite direto.
	/// </summary>
	DirectInvite = 3,
	/// <summary>
	/// Link administrativo.
	/// </summary>
	AdminLink = 4
}
