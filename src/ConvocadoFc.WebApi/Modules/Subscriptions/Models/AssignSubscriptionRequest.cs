using System;

namespace ConvocadoFc.WebApi.Modules.Subscriptions.Models;

public sealed record AssignSubscriptionRequest(
    Guid UserId,
    Guid PlanId,
    DateTimeOffset? StartsAt,
    DateTimeOffset? EndsAt,
    bool AutoRenew,
    string? Note
);
