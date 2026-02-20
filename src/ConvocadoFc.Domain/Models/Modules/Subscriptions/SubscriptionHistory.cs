using System;

using ConvocadoFc.Domain.Models.Modules.Users.Identity;

namespace ConvocadoFc.Domain.Models.Modules.Subscriptions;

public sealed class SubscriptionHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SubscriptionId { get; set; }
    public Guid OwnerUserId { get; set; }
    public Guid? OldPlanId { get; set; }
    public Guid? NewPlanId { get; set; }
    public SubscriptionStatus? OldStatus { get; set; }
    public SubscriptionStatus NewStatus { get; set; }
    public SubscriptionHistoryAction Action { get; set; }
    public Guid ChangedByUserId { get; set; }
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
    public string? Note { get; set; }

    public Subscription? Subscription { get; set; }
    public Plan? OldPlan { get; set; }
    public Plan? NewPlan { get; set; }
    public ApplicationUser? OwnerUser { get; set; }
    public ApplicationUser? ChangedByUser { get; set; }
}
