using System;

using ConvocadoFc.Domain.Models.Modules.Subscriptions;

namespace ConvocadoFc.WebApi.Modules.Subscriptions.Models;

public sealed record SubscriptionResponse(
    Guid Id,
    Guid OwnerUserId,
    Guid PlanId,
    string PlanName,
    string PlanCode,
    SubscriptionStatus Status,
    DateTimeOffset StartsAt,
    DateTimeOffset? EndsAt,
    bool AutoRenew,
    PlanCapacityResponse Capacity
);
