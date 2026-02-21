using ConvocadoFc.Application.Abstractions;
using ConvocadoFc.Application.Handlers.Modules.Subscriptions.Interfaces;
using ConvocadoFc.Application.Handlers.Modules.Subscriptions.Models;
using ConvocadoFc.Domain.Models.Modules.Subscriptions;

using Microsoft.EntityFrameworkCore;

namespace ConvocadoFc.Application.Handlers.Modules.Subscriptions.Implementations;

public sealed class SubscriptionAccessService(IApplicationDbContext dbContext) : ISubscriptionAccessService
{
    private readonly IApplicationDbContext _dbContext = dbContext;

    public async Task<SubscriptionAccessInfo> GetAccessInfoAsync(Guid ownerUserId, CancellationToken cancellationToken)
    {
        var subscriptionQuery = _dbContext.Query<Subscription>()
            .Where(subscription => subscription.OwnerUserId == ownerUserId
                                   && subscription.Status == ESubscriptionStatus.Active)
            .OrderByDescending(subscription => subscription.StartsAt);

        var data = await (from subscription in subscriptionQuery
                          join plan in _dbContext.Query<Plan>() on subscription.PlanId equals plan.Id
                          select new { subscription, plan })
            .FirstOrDefaultAsync(cancellationToken);

        if (data is null)
        {
            return new SubscriptionAccessInfo(false, null, null, null);
        }

        return new SubscriptionAccessInfo(
            true,
            data.plan.Id,
            data.plan.Code,
            new PlanCapacityDto(data.plan.MaxTeams, data.plan.MaxMembersPerTeam));
    }

    public async Task<bool> CanCreateTeamAsync(Guid ownerUserId, CancellationToken cancellationToken)
    {
        var accessInfo = await GetAccessInfoAsync(ownerUserId, cancellationToken);
        return accessInfo.HasActiveSubscription;
    }
}
