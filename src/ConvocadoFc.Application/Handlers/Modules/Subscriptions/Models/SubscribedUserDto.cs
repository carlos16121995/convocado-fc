using System;

namespace ConvocadoFc.Application.Handlers.Modules.Subscriptions.Models;

public sealed record SubscribedUserDto(
    Guid UserId,
    string Email,
    string FullName,
    SubscriptionDto Subscription
);
