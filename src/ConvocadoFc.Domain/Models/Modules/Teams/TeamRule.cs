namespace ConvocadoFc.Domain.Models.Modules.Teams;

public sealed class TeamRule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TeamSettingsId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Scope { get; set; }
    public string? Target { get; set; }
    public bool IsEnabled { get; set; } = true;
    public DateTimeOffset? StartsAt { get; set; }
    public DateTimeOffset? EndsAt { get; set; }

    public TeamSettings? TeamSettings { get; set; }
    public ICollection<TeamRuleParameter> Parameters { get; set; } = new List<TeamRuleParameter>();
}
