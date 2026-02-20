using ConvocadoFc.Domain.Models.Modules.Subscriptions;
using ConvocadoFc.WebApi.Models;

namespace ConvocadoFc.WebApi.Modules.Subscriptions.Models;

public sealed record ListSubscribedUsersQueryModel : PaginationQueryModel
{
    public SubscriptionStatus? Status { get; init; }
}
