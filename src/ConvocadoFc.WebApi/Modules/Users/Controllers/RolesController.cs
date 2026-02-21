using ConvocadoFc.Application.Handlers.Modules.Users.Interfaces;
using ConvocadoFc.Application.Handlers.Modules.Users.Models;
using ConvocadoFc.Domain.Models.Modules.Users.Identity;
using ConvocadoFc.Domain.Shared;
using ConvocadoFc.WebApi.Modules.Users.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConvocadoFc.WebApi.Modules.Users.Controllers;

/// <summary>
/// Endpoints de gerenciamento de roles do sistema.
/// Controla permissões globais de acesso.
/// </summary>
[ApiController]
[Route("api")]
public sealed class RolesController(IUserRolesHandler userRolesHandler) : ControllerBase
{
    private readonly IUserRolesHandler _userRolesHandler = userRolesHandler;

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
        Data = _userRolesHandler.ListRoles().ToList()
    });

    /// <summary>
    /// Atribui uma role a um usuário.
    /// Requer permissões administrativas.
    /// </summary>
    [HttpPost("users/{userId:guid}/roles")]
    [Authorize(Roles = SystemRoles.AdminOrMaster, Policy = AuthPolicies.EmailConfirmed)]
    public async Task<IActionResult> AssignRole([FromRoute] Guid userId, [FromBody] AssignRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await _userRolesHandler.AssignRoleAsync(new AssignUserRoleCommand(
            userId,
            request.Role,
            User.IsInRole(SystemRoles.Master),
            User.IsInRole(SystemRoles.Admin)),
            cancellationToken);

        if (result.Status == EUserRoleOperationStatus.InvalidRole)
        {
            return BadRequest(new ApiResponse
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Success = false,
                Message = "Role inválida."
            });
        }

        if (result.Status == EUserRoleOperationStatus.Forbidden)
        {
            return Forbid();
        }

        if (result.Status == EUserRoleOperationStatus.UserNotFound)
        {
            return NotFound(new ApiResponse
            {
                StatusCode = StatusCodes.Status404NotFound,
                Success = false,
                Message = "Usuário não encontrado."
            });
        }

        if (result.Status == EUserRoleOperationStatus.RoleNotConfigured)
        {
            return BadRequest(new ApiResponse
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Success = false,
                Message = "Role não configurada no sistema."
            });
        }

        if (result.Status != EUserRoleOperationStatus.Success)
        {
            return BadRequest(ToApiResponse(result.Errors));
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
        var result = await _userRolesHandler.RemoveRoleAsync(new RemoveUserRoleCommand(
            userId,
            role,
            User.IsInRole(SystemRoles.Master),
            User.IsInRole(SystemRoles.Admin)),
            cancellationToken);

        if (result.Status == EUserRoleOperationStatus.InvalidRole)
        {
            return BadRequest(new ApiResponse
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Success = false,
                Message = "Role inválida."
            });
        }

        if (result.Status == EUserRoleOperationStatus.Forbidden)
        {
            return Forbid();
        }

        if (result.Status == EUserRoleOperationStatus.UserNotFound)
        {
            return NotFound(new ApiResponse
            {
                StatusCode = StatusCodes.Status404NotFound,
                Success = false,
                Message = "Usuário não encontrado."
            });
        }

        if (result.Status != EUserRoleOperationStatus.Success)
        {
            return BadRequest(ToApiResponse(result.Errors));
        }

        return Ok(new ApiResponse
        {
            StatusCode = StatusCodes.Status200OK,
            Success = true,
            Message = "Role removida com sucesso."
        });
    }

    private static ApiResponse ToApiResponse(IReadOnlyCollection<ValidationFailure> errors) => new ApiResponse
    {
        StatusCode = StatusCodes.Status400BadRequest,
        Success = false,
        Message = "Operação não concluída.",
        Errors = errors.ToList()
    };
}
