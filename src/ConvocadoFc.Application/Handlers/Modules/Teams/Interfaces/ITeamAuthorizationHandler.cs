using ConvocadoFc.Application.Handlers.Modules.Teams.Models;

namespace ConvocadoFc.Application.Handlers.Modules.Teams.Interfaces;

public interface ITeamAuthorizationHandler
{
    Task<IReadOnlyCollection<TeamModeratorDto>> ListModeratorsAsync(Guid teamId, Guid currentUserId, bool isSystemAdmin, CancellationToken cancellationToken);
    Task<TeamAuthorizationOperationResult> AssignModeratorAsync(AssignModeratorCommand command, CancellationToken cancellationToken);
    Task<TeamAuthorizationOperationResult> RemoveModeratorAsync(RemoveModeratorCommand command, CancellationToken cancellationToken);
}
