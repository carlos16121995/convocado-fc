namespace ConvocadoFc.Application.Handlers.Modules.Subscriptions.Models;

public sealed record SubscriptionOperationResult(
    SubscriptionOperationStatus Status,
    SubscriptionDto? Subscription
);
