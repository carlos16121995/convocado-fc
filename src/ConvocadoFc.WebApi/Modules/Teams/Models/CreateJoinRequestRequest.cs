using ConvocadoFc.Domain.Models.Modules.Teams;

namespace ConvocadoFc.WebApi.Modules.Teams.Models;

/// <summary>
/// Solicitação para entrar em um time.
/// </summary>
/// <param name="Message">Mensagem enviada ao time.</param>
/// <param name="InviteId">Identificador do convite relacionado.</param>
/// <param name="Source">Origem da solicitação.</param>
public sealed record CreateJoinRequestRequest(
    string? Message,
    Guid? InviteId,
    ETeamJoinRequestSource Source
);
