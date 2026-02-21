using ConvocadoFc.Domain.Models.Modules.Teams;

namespace ConvocadoFc.WebApi.Modules.Teams.Models;

/// <summary>
/// Dados de uma solicitação de entrada em time.
/// </summary>
/// <param name="Id">Identificador da solicitação.</param>
/// <param name="TeamId">Identificador do time.</param>
/// <param name="UserId">Identificador do usuário solicitante.</param>
/// <param name="InviteId">Identificador do convite relacionado.</param>
/// <param name="ReviewedByUserId">Identificador do usuário que revisou.</param>
/// <param name="Status">Status da solicitação.</param>
/// <param name="Source">Origem da solicitação.</param>
/// <param name="IsAutoApproved">Indica se foi aprovada automaticamente.</param>
/// <param name="Message">Mensagem enviada pelo usuário.</param>
/// <param name="RequestedAt">Data de solicitação.</param>
/// <param name="ReviewedAt">Data de revisão.</param>
public sealed record JoinRequestResponse(
    Guid Id,
    Guid TeamId,
    Guid UserId,
    Guid? InviteId,
    Guid? ReviewedByUserId,
    ETeamJoinRequestStatus Status,
    ETeamJoinRequestSource Source,
    bool IsAutoApproved,
    string? Message,
    DateTimeOffset RequestedAt,
    DateTimeOffset? ReviewedAt
);
