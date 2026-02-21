namespace ConvocadoFc.Domain.Models.Modules.Teams;

/// <summary>
/// Canais de envio de convites.
/// </summary>
public enum ETeamInviteChannel
{
	/// <summary>
	/// Envio por e-mail.
	/// </summary>
	Email = 0,
	/// <summary>
	/// Link compartilhável.
	/// </summary>
	ShareLink = 1,
	/// <summary>
	/// QR Code.
	/// </summary>
	QrCode = 2
}
