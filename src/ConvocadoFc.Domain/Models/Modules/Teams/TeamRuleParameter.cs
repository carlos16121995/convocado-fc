namespace ConvocadoFc.Domain.Models.Modules.Teams;

public sealed class TeamRuleParameter
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TeamRuleId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? ValueType { get; set; }
    public string? Unit { get; set; }
    public string? Description { get; set; }

    public TeamRule? TeamRule { get; set; }
}
