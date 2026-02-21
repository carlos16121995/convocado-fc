using ConvocadoFc.Application.Handlers.Modules.Teams.Models;
using ConvocadoFc.Domain.Shared;

namespace ConvocadoFc.Application.Handlers.Modules.Teams.Interfaces;

public interface ITeamPlayerHandler
{
    Task<PaginatedResult<TeamPlayerDto>> ListPlayersAsync(ListTeamPlayersQuery query, CancellationToken cancellationToken);
    Task<TeamPlayerOperationResult> UpdateMyProfileAsync(UpdateMyProfileCommand command, CancellationToken cancellationToken);
    Task<TeamPlayerOperationResult> UpdatePlayerAdminAsync(UpdatePlayerAdminCommand command, CancellationToken cancellationToken);
    Task<TeamPlayerOperationResult> RemovePlayerAsync(RemovePlayerCommand command, CancellationToken cancellationToken);
}
