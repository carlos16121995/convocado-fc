using ConvocadoFc.Application.Handlers.Modules.Teams.Models;
using ConvocadoFc.Domain.Shared;

namespace ConvocadoFc.Application.Handlers.Modules.Teams.Interfaces;

public interface ITeamManagementHandler
{
    Task<PaginatedResult<TeamDto>> ListTeamsAsync(ListTeamsQuery query, CancellationToken cancellationToken);
    Task<TeamDto?> GetTeamAsync(Guid teamId, CancellationToken cancellationToken);
    Task<TeamOperationResult> CreateTeamAsync(CreateTeamCommand command, CancellationToken cancellationToken);
    Task<TeamOperationResult> UpdateTeamAsync(UpdateTeamCommand command, CancellationToken cancellationToken);
    Task<TeamOperationResult> RemoveTeamAsync(Guid teamId, Guid removedByUserId, bool isSystemAdmin, CancellationToken cancellationToken);
}
