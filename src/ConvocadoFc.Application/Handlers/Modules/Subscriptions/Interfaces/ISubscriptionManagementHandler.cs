using ConvocadoFc.Application.Handlers.Modules.Subscriptions.Models;
using ConvocadoFc.Domain.Shared;

namespace ConvocadoFc.Application.Handlers.Modules.Subscriptions.Interfaces;

public interface ISubscriptionManagementHandler
{
    Task<IReadOnlyCollection<SubscriptionDto>> ListSubscriptionsAsync(ListSubscriptionsQuery query, CancellationToken cancellationToken);
    Task<PaginatedResult<SubscribedUserDto>> ListSubscribedUsersAsync(ListSubscribedUsersQuery query, CancellationToken cancellationToken);
    Task<SubscriptionOperationResult> AssignSubscriptionAsync(AssignSubscriptionCommand command, CancellationToken cancellationToken);
    Task<SubscriptionOperationResult> ChangeSubscriptionAsync(ChangeSubscriptionCommand command, CancellationToken cancellationToken);
    Task<SubscriptionOperationResult> RemoveSubscriptionAsync(RemoveSubscriptionCommand command, CancellationToken cancellationToken);
}
