using ConvocadoFc.Application.Handlers.Modules.Subscriptions.Models;

namespace ConvocadoFc.Application.Handlers.Modules.Subscriptions.Interfaces;

public interface ISubscriptionAccessService
{
    Task<SubscriptionAccessInfo> GetAccessInfoAsync(Guid ownerUserId, CancellationToken cancellationToken);
    Task<bool> CanCreateTeamAsync(Guid ownerUserId, CancellationToken cancellationToken);
}
