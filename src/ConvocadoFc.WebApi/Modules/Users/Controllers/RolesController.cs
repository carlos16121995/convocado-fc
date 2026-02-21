using ConvocadoFc.Domain.Models.Modules.Users.Identity;
using ConvocadoFc.Domain.Shared;
using ConvocadoFc.WebApi.Modules.Users.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ConvocadoFc.WebApi.Modules.Users.Controllers;

/// <summary>
/// Endpoints de gerenciamento de roles do sistema.
/// Controla permissões globais de acesso.
/// </summary>
[ApiController]
[Route("api")]
public sealed class RolesController(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager) : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly RoleManager<ApplicationRole> _roleManager = roleManager;

    /// <summary>
    /// Lista as roles disponíveis no sistema.
    /// Útil para interfaces administrativas.
    /// </summary>
    [HttpGet("roles")]
    [Authorize(Roles = SystemRoles.ModeratorAdminMaster, Policy = AuthPolicies.EmailConfirmed)]
    public IActionResult ListRoles() => Ok(new ApiResponse<IReadOnlyCollection<string>>
    {
        StatusCode = StatusCodes.Status200OK,
        Success = true,
        Message = "Roles disponíveis.",
        Data = SystemRoles.All.ToList()
    });

    /// <summary>
    /// Atribui uma role a um usuário.
    /// Requer permissões administrativas.
    /// </summary>
    [HttpPost("users/{userId:guid}/roles")]
    [Authorize(Roles = SystemRoles.AdminOrMaster, Policy = AuthPolicies.EmailConfirmed)]
    public async Task<IActionResult> AssignRole([FromRoute] Guid userId, [FromBody] AssignRoleRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var normalizedRole = NormalizeRole(request.Role);
        if (!SystemRoles.All.Contains(normalizedRole))
        {
            return BadRequest(new ApiResponse
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Success = false,
                Message = "Role inválida."
            });
        }

        if (!await CanManageRoleAsync(normalizedRole))
        {
            return Forbid();
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return NotFound(new ApiResponse
            {
                StatusCode = StatusCodes.Status404NotFound,
                Success = false,
                Message = "Usuário não encontrado."
            });
        }

        if (!await _roleManager.RoleExistsAsync(normalizedRole))
        {
            return BadRequest(new ApiResponse
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Success = false,
                Message = "Role não configurada no sistema."
            });
        }

        var result = await _userManager.AddToRoleAsync(user, normalizedRole);
        if (!result.Succeeded)
        {
            return BadRequest(ToApiResponse(result));
        }

        return Ok(new ApiResponse
        {
            StatusCode = StatusCodes.Status200OK,
            Success = true,
            Message = "Role atribuída com sucesso."
        });
    }

    /// <summary>
    /// Remove uma role de um usuário.
    /// Requer permissões administrativas.
    /// </summary>
    [HttpDelete("users/{userId:guid}/roles/{role}")]
    [Authorize(Roles = SystemRoles.AdminOrMaster, Policy = AuthPolicies.EmailConfirmed)]
    public async Task<IActionResult> RemoveRole([FromRoute] Guid userId, [FromRoute] string role, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var normalizedRole = NormalizeRole(role);
        if (!SystemRoles.All.Contains(normalizedRole))
        {
            return BadRequest(new ApiResponse
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Success = false,
                Message = "Role inválida."
            });
        }

        if (!await CanManageRoleAsync(normalizedRole))
        {
            return Forbid();
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return NotFound(new ApiResponse
            {
                StatusCode = StatusCodes.Status404NotFound,
                Success = false,
                Message = "Usuário não encontrado."
            });
        }

        var result = await _userManager.RemoveFromRoleAsync(user, normalizedRole);
        if (!result.Succeeded)
        {
            return BadRequest(ToApiResponse(result));
        }

        return Ok(new ApiResponse
        {
            StatusCode = StatusCodes.Status200OK,
            Success = true,
            Message = "Role removida com sucesso."
        });
    }

    private async Task<bool> CanManageRoleAsync(string role)
    {
        if (role == SystemRoles.Master)
        {
            return User.IsInRole(SystemRoles.Master);
        }

        if (role == SystemRoles.Admin)
        {
            return User.IsInRole(SystemRoles.Master);
        }

        return User.IsInRole(SystemRoles.Master) || User.IsInRole(SystemRoles.Admin);
    }

    private static string NormalizeRole(string role) => role.Trim();

    private static ApiResponse ToApiResponse(IdentityResult result) => new ApiResponse
    {
        StatusCode = StatusCodes.Status400BadRequest,
        Success = false,
        Message = "Operação não concluída.",
        Errors = result.Errors.Select(error => new ValidationFailure
        {
            PropertyName = error.Code,
            ErrorMessage = error.Description
        }).ToList()
    };
}
