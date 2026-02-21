namespace ConvocadoFc.WebApi.Modules.Teams.Models;

/// <summary>
/// Conjunto de configurações e regras do time.
/// </summary>
/// <param name="TeamId">Identificador do time.</param>
/// <param name="Settings">Lista de configurações.</param>
/// <param name="Rules">Lista de regras.</param>
public sealed record TeamSettingsResponse(
    Guid TeamId,
    IReadOnlyCollection<TeamSettingEntryResponse> Settings,
    IReadOnlyCollection<TeamRuleResponse> Rules
);

/// <summary>
/// Configuração individual do time.
/// </summary>
/// <param name="Id">Identificador da configuração.</param>
/// <param name="Key">Chave da configuração.</param>
/// <param name="Value">Valor da configuração.</param>
/// <param name="ValueType">Tipo do valor armazenado.</param>
/// <param name="IsEnabled">Indica se a configuração está ativa.</param>
/// <param name="Description">Descrição da configuração.</param>
public sealed record TeamSettingEntryResponse(
    Guid Id,
    string Key,
    string Value,
    string? ValueType,
    bool IsEnabled,
    string? Description
);

/// <summary>
/// Regra configurada para o time.
/// </summary>
/// <param name="Id">Identificador da regra.</param>
/// <param name="Code">Código da regra.</param>
/// <param name="Name">Nome da regra.</param>
/// <param name="Description">Descrição da regra.</param>
/// <param name="Scope">Escopo de aplicação.</param>
/// <param name="Target">Alvo de aplicação.</param>
/// <param name="IsEnabled">Indica se a regra está ativa.</param>
/// <param name="StartsAt">Data de início da regra.</param>
/// <param name="EndsAt">Data de término da regra.</param>
/// <param name="Parameters">Parâmetros da regra.</param>
public sealed record TeamRuleResponse(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    string? Scope,
    string? Target,
    bool IsEnabled,
    DateTimeOffset? StartsAt,
    DateTimeOffset? EndsAt,
    IReadOnlyCollection<TeamRuleParameterResponse> Parameters
);

/// <summary>
/// Parâmetro de uma regra do time.
/// </summary>
/// <param name="Id">Identificador do parâmetro.</param>
/// <param name="Key">Chave do parâmetro.</param>
/// <param name="Value">Valor do parâmetro.</param>
/// <param name="ValueType">Tipo do valor.</param>
/// <param name="Unit">Unidade de medida.</param>
/// <param name="Description">Descrição do parâmetro.</param>
public sealed record TeamRuleParameterResponse(
    Guid Id,
    string Key,
    string Value,
    string? ValueType,
    string? Unit,
    string? Description
);
