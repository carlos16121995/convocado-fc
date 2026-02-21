namespace ConvocadoFc.Application.Handlers.Modules.Subscriptions.Models;

public sealed record SubscriptionOperationResult(
    ESubscriptionOperationStatus Status,
    SubscriptionDto? Subscription
);
