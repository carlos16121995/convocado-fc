using ConvocadoFc.Application.Handlers.Modules.Users.Interfaces;
using ConvocadoFc.Application.Handlers.Modules.Users.Models;
using ConvocadoFc.Domain.Models.Modules.Users.Identity;
using ConvocadoFc.Domain.Shared;

using Microsoft.AspNetCore.Identity;

namespace ConvocadoFc.Application.Handlers.Modules.Users.Implementations;

public sealed class UserRolesHandler(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager) : IUserRolesHandler
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly RoleManager<ApplicationRole> _roleManager = roleManager;

    public IReadOnlyCollection<string> ListRoles() => SystemRoles.All.ToList();

    public async Task<UserRoleOperationResult> AssignRoleAsync(AssignUserRoleCommand command, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var normalizedRole = NormalizeRole(command.Role);
        if (!SystemRoles.All.Contains(normalizedRole))
        {
            return UserRoleOperationResult.Failure(EUserRoleOperationStatus.InvalidRole);
        }

        if (!CanManageRole(normalizedRole, command.IsMaster, command.IsAdmin))
        {
            return UserRoleOperationResult.Failure(EUserRoleOperationStatus.Forbidden);
        }

        var user = await _userManager.FindByIdAsync(command.UserId.ToString());
        if (user is null)
        {
            return UserRoleOperationResult.Failure(EUserRoleOperationStatus.UserNotFound);
        }

        if (!await _roleManager.RoleExistsAsync(normalizedRole))
        {
            return UserRoleOperationResult.Failure(EUserRoleOperationStatus.RoleNotConfigured);
        }

        var result = await _userManager.AddToRoleAsync(user, normalizedRole);
        if (!result.Succeeded)
        {
            return UserRoleOperationResult.Failure(EUserRoleOperationStatus.Failed, ToValidationFailures(result));
        }

        return UserRoleOperationResult.Success();
    }

    public async Task<UserRoleOperationResult> RemoveRoleAsync(RemoveUserRoleCommand command, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var normalizedRole = NormalizeRole(command.Role);
        if (!SystemRoles.All.Contains(normalizedRole))
        {
            return UserRoleOperationResult.Failure(EUserRoleOperationStatus.InvalidRole);
        }

        if (!CanManageRole(normalizedRole, command.IsMaster, command.IsAdmin))
        {
            return UserRoleOperationResult.Failure(EUserRoleOperationStatus.Forbidden);
        }

        var user = await _userManager.FindByIdAsync(command.UserId.ToString());
        if (user is null)
        {
            return UserRoleOperationResult.Failure(EUserRoleOperationStatus.UserNotFound);
        }

        var result = await _userManager.RemoveFromRoleAsync(user, normalizedRole);
        if (!result.Succeeded)
        {
            return UserRoleOperationResult.Failure(EUserRoleOperationStatus.Failed, ToValidationFailures(result));
        }

        return UserRoleOperationResult.Success();
    }

    private static bool CanManageRole(string role, bool isMaster, bool isAdmin)
    {
        if (role == SystemRoles.Master)
        {
            return isMaster;
        }

        if (role == SystemRoles.Admin)
        {
            return isMaster;
        }

        return isMaster || isAdmin;
    }

    private static string NormalizeRole(string role) => role.Trim();

    private static IReadOnlyCollection<ValidationFailure> ToValidationFailures(IdentityResult result)
        => result.Errors.Select(error => new ValidationFailure
        {
            PropertyName = error.Code,
            ErrorMessage = error.Description
        }).ToList();
}
