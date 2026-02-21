namespace ConvocadoFc.Application.Handlers.Modules.Teams.Models;

public sealed record UpsertTeamSettingsCommand(
    Guid TeamId,
    Guid UpdatedByUserId,
    IReadOnlyCollection<UpsertTeamSettingEntry> Settings,
    bool IsSystemAdmin
);

public sealed record UpsertTeamSettingEntry(
    string Key,
    string Value,
    string? ValueType,
    bool IsEnabled,
    string? Description
);
