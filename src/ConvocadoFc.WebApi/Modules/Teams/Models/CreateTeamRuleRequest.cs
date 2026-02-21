namespace ConvocadoFc.WebApi.Modules.Teams.Models;

/// <summary>
/// Solicitação para criação de regra do time.
/// </summary>
/// <param name="Code">Código da regra.</param>
/// <param name="Name">Nome da regra.</param>
/// <param name="Description">Descrição da regra.</param>
/// <param name="Scope">Escopo de aplicação.</param>
/// <param name="Target">Alvo da regra.</param>
/// <param name="IsEnabled">Indica se a regra está ativa.</param>
/// <param name="StartsAt">Data de início.</param>
/// <param name="EndsAt">Data de término.</param>
public sealed record CreateTeamRuleRequest(
    string Code,
    string Name,
    string? Description,
    string? Scope,
    string? Target,
    bool IsEnabled,
    DateTimeOffset? StartsAt,
    DateTimeOffset? EndsAt
);
