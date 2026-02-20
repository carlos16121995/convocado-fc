using System;

using ConvocadoFc.Domain.Models.Modules.Subscriptions;

namespace ConvocadoFc.Application.Handlers.Modules.Subscriptions.Models;

public sealed record ChangeSubscriptionCommand(
    Guid SubscriptionId,
    Guid? PlanId,
    DateTimeOffset? EndsAt,
    bool? AutoRenew,
    SubscriptionStatus? Status,
    Guid ChangedByUserId,
    string? Note
);
