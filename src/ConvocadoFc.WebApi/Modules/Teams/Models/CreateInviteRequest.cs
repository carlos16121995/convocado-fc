using ConvocadoFc.Domain.Models.Modules.Teams;

namespace ConvocadoFc.WebApi.Modules.Teams.Models;

/// <summary>
/// Solicitação para criação de convite do time.
/// </summary>
/// <param name="TargetUserId">Identificador do usuário convidado.</param>
/// <param name="TargetEmail">E-mail do convidado.</param>
/// <param name="Channel">Canal de envio do convite.</param>
/// <param name="MaxUses">Quantidade máxima de usos do convite.</param>
/// <param name="ExpiresAt">Data de expiração do convite.</param>
/// <param name="Message">Mensagem opcional do convite.</param>
public sealed record CreateInviteRequest(
    Guid? TargetUserId,
    string? TargetEmail,
    ETeamInviteChannel Channel,
    int? MaxUses,
    DateTimeOffset? ExpiresAt,
    string? Message
);
