namespace ConvocadoFc.WebApi.Modules.Teams.Models;

/// <summary>
/// Solicitação para adicionar parâmetro em uma regra.
/// </summary>
/// <param name="Key">Chave do parâmetro.</param>
/// <param name="Value">Valor do parâmetro.</param>
/// <param name="ValueType">Tipo do valor.</param>
/// <param name="Unit">Unidade de medida.</param>
/// <param name="Description">Descrição do parâmetro.</param>
public sealed record AddTeamRuleParameterRequest(
    string Key,
    string Value,
    string? ValueType,
    string? Unit,
    string? Description
);
