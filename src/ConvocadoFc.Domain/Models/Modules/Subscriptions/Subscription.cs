using ConvocadoFc.Domain.Models.Modules.Users.Identity;

namespace ConvocadoFc.Domain.Models.Modules.Subscriptions;

public sealed class Subscription
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OwnerUserId { get; set; }
    public Guid PlanId { get; set; }
    public ESubscriptionStatus Status { get; set; } = ESubscriptionStatus.Active;
    public DateTimeOffset StartsAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? EndsAt { get; set; }
    public bool AutoRenew { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? CanceledAt { get; set; }
    public string? Notes { get; set; }

    public Plan? Plan { get; set; }
    public ApplicationUser? OwnerUser { get; set; }
    public ICollection<SubscriptionHistory> Histories { get; set; } = new List<SubscriptionHistory>();
}
