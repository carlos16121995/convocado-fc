using ConvocadoFc.Domain.Shared;

namespace ConvocadoFc.Application.Handlers.Modules.Teams.Models;

public sealed record ListMyInvitesQuery(
    PaginationQuery Pagination,
    Guid UserId
);
