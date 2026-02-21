namespace ConvocadoFc.Domain.Models.Modules.Teams;

public sealed class TeamSettingEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TeamSettingsId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? ValueType { get; set; }
    public bool IsEnabled { get; set; } = true;
    public string? Description { get; set; }

    public TeamSettings? TeamSettings { get; set; }
}
