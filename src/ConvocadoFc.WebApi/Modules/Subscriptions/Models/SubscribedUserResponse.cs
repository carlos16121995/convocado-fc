using System;

namespace ConvocadoFc.WebApi.Modules.Subscriptions.Models;

public sealed record SubscribedUserResponse(
    Guid UserId,
    string Email,
    string FullName,
    SubscriptionResponse Subscription
);
