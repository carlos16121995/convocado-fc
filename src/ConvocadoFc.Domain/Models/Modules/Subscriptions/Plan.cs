namespace ConvocadoFc.Domain.Models.Modules.Subscriptions;

public sealed class Plan
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public decimal? Price { get; set; }
    public string Currency { get; set; } = "BRL";
    public int? MaxTeams { get; set; }
    public int? MaxMembersPerTeam { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsCustomPricing { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }

    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
