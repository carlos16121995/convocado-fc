using ConvocadoFc.Domain.Shared;

namespace ConvocadoFc.Application.Handlers.Modules.Teams.Models;

public sealed record ListTeamPlayersQuery(
    PaginationQuery Pagination,
    Guid TeamId,
    Guid CurrentUserId,
    bool IsSystemAdmin
);
