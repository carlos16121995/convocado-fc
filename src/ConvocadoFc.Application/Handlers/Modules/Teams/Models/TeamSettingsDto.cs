namespace ConvocadoFc.Application.Handlers.Modules.Teams.Models;

public sealed record TeamSettingsDto(
    Guid TeamId,
    IReadOnlyCollection<TeamSettingEntryDto> Settings,
    IReadOnlyCollection<TeamRuleDto> Rules
);

public sealed record TeamSettingEntryDto(
    Guid Id,
    string Key,
    string Value,
    string? ValueType,
    bool IsEnabled,
    string? Description
);

public sealed record TeamRuleDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    string? Scope,
    string? Target,
    bool IsEnabled,
    DateTimeOffset? StartsAt,
    DateTimeOffset? EndsAt,
    IReadOnlyCollection<TeamRuleParameterDto> Parameters
);

public sealed record TeamRuleParameterDto(
    Guid Id,
    string Key,
    string Value,
    string? ValueType,
    string? Unit,
    string? Description
);
