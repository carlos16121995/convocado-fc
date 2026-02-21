namespace ConvocadoFc.WebApi.Modules.Teams.Models;

/// <summary>
/// Solicitação para atualização administrativa do jogador.
/// </summary>
/// <param name="IsFeeExempt">Indica se o jogador é isento de mensalidade.</param>
public sealed record UpdatePlayerAdminRequest(
    bool? IsFeeExempt
);
