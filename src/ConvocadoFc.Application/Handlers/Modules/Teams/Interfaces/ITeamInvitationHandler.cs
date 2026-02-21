using ConvocadoFc.Application.Handlers.Modules.Teams.Models;
using ConvocadoFc.Domain.Shared;

namespace ConvocadoFc.Application.Handlers.Modules.Teams.Interfaces;

public interface ITeamInvitationHandler
{
    Task<PaginatedResult<TeamInviteDto>> ListSentInvitesAsync(ListSentInvitesQuery query, CancellationToken cancellationToken);
    Task<PaginatedResult<TeamInviteDto>> ListMyInvitesAsync(ListMyInvitesQuery query, CancellationToken cancellationToken);
    Task<TeamInviteOperationResult> CreateInviteAsync(CreateInviteCommand command, CancellationToken cancellationToken);
    Task<TeamInviteOperationResult> AcceptInviteAsync(AcceptInviteCommand command, CancellationToken cancellationToken);
    Task<PaginatedResult<TeamJoinRequestDto>> ListJoinRequestsAsync(ListJoinRequestsQuery query, CancellationToken cancellationToken);
    Task<TeamJoinRequestOperationResult> CreateJoinRequestAsync(CreateJoinRequestCommand command, CancellationToken cancellationToken);
    Task<TeamJoinRequestOperationResult> ReviewJoinRequestAsync(ReviewJoinRequestCommand command, CancellationToken cancellationToken);
}
