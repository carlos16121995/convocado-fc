namespace ConvocadoFc.Domain.Models.Modules.Teams;

public sealed class TeamSettings
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TeamId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }

    public Team? Team { get; set; }
    public ICollection<TeamSettingEntry> Settings { get; set; } = new List<TeamSettingEntry>();
    public ICollection<TeamRule> Rules { get; set; } = new List<TeamRule>();
}
