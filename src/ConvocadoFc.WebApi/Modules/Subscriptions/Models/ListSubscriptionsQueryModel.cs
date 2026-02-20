using System;

using ConvocadoFc.Domain.Models.Modules.Subscriptions;

namespace ConvocadoFc.WebApi.Modules.Subscriptions.Models;

public sealed record ListSubscriptionsQueryModel
{
    public Guid? UserId { get; init; }
    public SubscriptionStatus? Status { get; init; }
    public Guid? PlanId { get; init; }
}
