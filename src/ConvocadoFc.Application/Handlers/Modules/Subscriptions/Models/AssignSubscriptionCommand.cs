using System;

namespace ConvocadoFc.Application.Handlers.Modules.Subscriptions.Models;

public sealed record AssignSubscriptionCommand(
    Guid OwnerUserId,
    Guid PlanId,
    DateTimeOffset? StartsAt,
    DateTimeOffset? EndsAt,
    bool AutoRenew,
    Guid AssignedByUserId,
    string? Note
);
