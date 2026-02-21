using ConvocadoFc.Domain.Models.Modules.Teams;
using ConvocadoFc.Domain.Shared;

namespace ConvocadoFc.Application.Handlers.Modules.Teams.Models;

public sealed record ListJoinRequestsQuery(
    PaginationQuery Pagination,
    Guid TeamId,
    Guid CurrentUserId,
    ETeamJoinRequestStatus? Status,
    bool IsSystemAdmin
);
