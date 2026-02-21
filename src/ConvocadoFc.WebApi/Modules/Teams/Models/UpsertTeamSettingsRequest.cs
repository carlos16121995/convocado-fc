namespace ConvocadoFc.WebApi.Modules.Teams.Models;

/// <summary>
/// Solicitação para criar ou atualizar configurações do time.
/// </summary>
/// <param name="Settings">Lista de configurações a persistir.</param>
public sealed record UpsertTeamSettingsRequest(
    IReadOnlyCollection<UpsertTeamSettingEntryRequest> Settings
);

/// <summary>
/// Item de configuração enviado para atualização.
/// </summary>
/// <param name="Key">Chave da configuração.</param>
/// <param name="Value">Valor da configuração.</param>
/// <param name="ValueType">Tipo do valor.</param>
/// <param name="IsEnabled">Indica se a configuração está ativa.</param>
/// <param name="Description">Descrição da configuração.</param>
public sealed record UpsertTeamSettingEntryRequest(
    string Key,
    string Value,
    string? ValueType,
    bool IsEnabled,
    string? Description
);
