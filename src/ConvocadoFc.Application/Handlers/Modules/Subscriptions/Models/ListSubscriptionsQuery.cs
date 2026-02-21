using ConvocadoFc.Domain.Models.Modules.Subscriptions;

namespace ConvocadoFc.Application.Handlers.Modules.Subscriptions.Models;

public sealed record ListSubscriptionsQuery(
    Guid? OwnerUserId,
    ESubscriptionStatus? Status,
    Guid? PlanId
);
