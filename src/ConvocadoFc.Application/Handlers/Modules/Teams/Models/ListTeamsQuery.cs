using ConvocadoFc.Domain.Shared;

namespace ConvocadoFc.Application.Handlers.Modules.Teams.Models;

public sealed record ListTeamsQuery(
    PaginationQuery Pagination,
    Guid? OwnerUserId
);
