using ConvocadoFc.Application.Handlers.Modules.Users.Models;

namespace ConvocadoFc.Application.Handlers.Modules.Users.Interfaces;

public interface IUserRolesHandler
{
    IReadOnlyCollection<string> ListRoles();
    Task<UserRoleOperationResult> AssignRoleAsync(AssignUserRoleCommand command, CancellationToken cancellationToken);
    Task<UserRoleOperationResult> RemoveRoleAsync(RemoveUserRoleCommand command, CancellationToken cancellationToken);
}
