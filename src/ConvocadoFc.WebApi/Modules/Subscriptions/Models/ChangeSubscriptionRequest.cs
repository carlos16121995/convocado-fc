using System;

using ConvocadoFc.Domain.Models.Modules.Subscriptions;

namespace ConvocadoFc.WebApi.Modules.Subscriptions.Models;

public sealed record ChangeSubscriptionRequest(
    Guid SubscriptionId,
    Guid? PlanId,
    DateTimeOffset? EndsAt,
    bool? AutoRenew,
    SubscriptionStatus? Status,
    string? Note
);
