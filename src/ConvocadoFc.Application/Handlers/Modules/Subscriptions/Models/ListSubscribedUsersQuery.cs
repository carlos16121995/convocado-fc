using ConvocadoFc.Domain.Shared;
using ConvocadoFc.Domain.Models.Modules.Subscriptions;

namespace ConvocadoFc.Application.Handlers.Modules.Subscriptions.Models;

public sealed record ListSubscribedUsersQuery(
    PaginationQuery Pagination,
    ESubscriptionStatus? Status
);
