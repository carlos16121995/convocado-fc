using ConvocadoFc.Domain.Models.Modules.Teams;

namespace ConvocadoFc.WebApi.Modules.Teams.Models;

/// <summary>
/// Dados de um convite para o time.
/// </summary>
/// <param name="Id">Identificador do convite.</param>
/// <param name="TeamId">Identificador do time.</param>
/// <param name="CreatedByUserId">Usuário que criou o convite.</param>
/// <param name="TargetUserId">Usuário alvo do convite.</param>
/// <param name="TargetEmail">E-mail alvo do convite.</param>
/// <param name="Token">Token do convite.</param>
/// <param name="Channel">Canal de envio.</param>
/// <param name="Status">Status do convite.</param>
/// <param name="IsPreApproved">Indica se o convite é pré-aprovado.</param>
/// <param name="MaxUses">Quantidade máxima de usos.</param>
/// <param name="UseCount">Quantidade de usos atuais.</param>
/// <param name="Message">Mensagem do convite.</param>
/// <param name="CreatedAt">Data de criação.</param>
/// <param name="ExpiresAt">Data de expiração.</param>
/// <param name="AcceptedAt">Data de aceite.</param>
public sealed record InviteResponse(
    Guid Id,
    Guid TeamId,
    Guid CreatedByUserId,
    Guid? TargetUserId,
    string? TargetEmail,
    string Token,
    ETeamInviteChannel Channel,
    ETeamInviteStatus Status,
    bool IsPreApproved,
    int? MaxUses,
    int UseCount,
    string? Message,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ExpiresAt,
    DateTimeOffset? AcceptedAt
);
