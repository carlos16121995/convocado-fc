using ConvocadoFc.Domain.Models.Modules.Teams;

namespace ConvocadoFc.WebApi.Modules.Teams.Models;

/// <summary>
/// Dados do jogador no time.
/// </summary>
/// <param name="TeamMemberId">Identificador do vínculo do jogador com o time.</param>
/// <param name="TeamId">Identificador do time.</param>
/// <param name="UserId">Identificador do usuário.</param>
/// <param name="FullName">Nome completo do jogador.</param>
/// <param name="Role">Role do jogador no time.</param>
/// <param name="Status">Status da participação no time.</param>
/// <param name="IsFeeExempt">Indica isenção de mensalidade.</param>
/// <param name="IsOnHiatus">Indica se o jogador está em hiato.</param>
/// <param name="HiatusStartedAt">Início do hiato.</param>
/// <param name="HiatusEndsAt">Término do hiato.</param>
/// <param name="PrimaryPosition">Posição principal.</param>
/// <param name="SecondaryPosition">Posição secundária.</param>
/// <param name="TertiaryPosition">Posição terciária.</param>
public sealed record PlayerResponse(
    Guid TeamMemberId,
    Guid TeamId,
    Guid UserId,
    string FullName,
    ETeamMemberRole Role,
    ETeamMemberStatus Status,
    bool IsFeeExempt,
    bool IsOnHiatus,
    DateTimeOffset? HiatusStartedAt,
    DateTimeOffset? HiatusEndsAt,
    EPlayerPosition? PrimaryPosition,
    EPlayerPosition? SecondaryPosition,
    EPlayerPosition? TertiaryPosition
);
