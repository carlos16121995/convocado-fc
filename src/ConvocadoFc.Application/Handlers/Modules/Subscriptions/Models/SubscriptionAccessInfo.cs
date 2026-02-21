namespace ConvocadoFc.Application.Handlers.Modules.Subscriptions.Models;

public sealed record SubscriptionAccessInfo(
    bool HasActiveSubscription,
    Guid? PlanId,
    string? PlanCode,
    PlanCapacityDto? Capacity
);
