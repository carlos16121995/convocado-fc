namespace ConvocadoFc.Application.Handlers.Modules.Subscriptions.Models;

public sealed record RemoveSubscriptionCommand(
    Guid SubscriptionId,
    Guid RemovedByUserId,
    string? Note
);
