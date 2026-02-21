namespace ConvocadoFc.Domain.Models.Modules.Teams;

/// <summary>
/// Papéis possíveis do membro dentro do time.
/// </summary>
public enum ETeamMemberRole
{
	/// <summary>
	/// Membro padrão do time.
	/// </summary>
	User = 0,
	/// <summary>
	/// Moderador com permissões intermediárias.
	/// </summary>
	Moderator = 1,
	/// <summary>
	/// Administrador do time.
	/// </summary>
	Admin = 2
}
