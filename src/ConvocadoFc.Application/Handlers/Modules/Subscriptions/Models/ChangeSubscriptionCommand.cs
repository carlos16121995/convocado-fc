using ConvocadoFc.Domain.Models.Modules.Subscriptions;

namespace ConvocadoFc.Application.Handlers.Modules.Subscriptions.Models;

public sealed record ChangeSubscriptionCommand(
    Guid SubscriptionId,
    Guid? PlanId,
    DateTimeOffset? EndsAt,
    bool? AutoRenew,
    ESubscriptionStatus? Status,
    Guid ChangedByUserId,
    string? Note
);
