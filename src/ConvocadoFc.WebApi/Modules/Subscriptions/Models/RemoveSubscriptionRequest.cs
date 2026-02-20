using System;

namespace ConvocadoFc.WebApi.Modules.Subscriptions.Models;

public sealed record RemoveSubscriptionRequest(
    Guid SubscriptionId,
    string? Note
);
