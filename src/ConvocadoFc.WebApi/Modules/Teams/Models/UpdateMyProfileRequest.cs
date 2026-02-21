using ConvocadoFc.Domain.Models.Modules.Teams;

namespace ConvocadoFc.WebApi.Modules.Teams.Models;

/// <summary>
/// Solicitação para atualizar o perfil do jogador no time.
/// </summary>
/// <param name="PrimaryPosition">Posição principal do jogador.</param>
/// <param name="SecondaryPosition">Posição secundária do jogador.</param>
/// <param name="TertiaryPosition">Posição terciária do jogador.</param>
/// <param name="CopyFromTeamId">Time de referência para copiar posições.</param>
/// <param name="IsOnHiatus">Indica se o jogador está em hiato.</param>
/// <param name="HiatusEndsAt">Data de término do hiato.</param>
public sealed record UpdateMyProfileRequest(
    EPlayerPosition? PrimaryPosition,
    EPlayerPosition? SecondaryPosition,
    EPlayerPosition? TertiaryPosition,
    Guid? CopyFromTeamId,
    bool? IsOnHiatus,
    DateTimeOffset? HiatusEndsAt
);
