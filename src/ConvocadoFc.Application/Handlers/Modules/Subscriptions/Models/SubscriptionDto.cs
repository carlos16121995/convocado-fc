using ConvocadoFc.Domain.Models.Modules.Subscriptions;

namespace ConvocadoFc.Application.Handlers.Modules.Subscriptions.Models;

public sealed record SubscriptionDto(
    Guid Id,
    Guid OwnerUserId,
    Guid PlanId,
    string PlanName,
    string PlanCode,
    ESubscriptionStatus Status,
    DateTimeOffset StartsAt,
    DateTimeOffset? EndsAt,
    bool AutoRenew,
    PlanCapacityDto Capacity
);
