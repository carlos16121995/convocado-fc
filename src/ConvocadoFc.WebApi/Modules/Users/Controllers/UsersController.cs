using ConvocadoFc.Application.Handlers.Modules.Users.Interfaces;
using ConvocadoFc.Application.Handlers.Modules.Users.Models;
using ConvocadoFc.Domain.Shared;
using ConvocadoFc.WebApi.Modules.Authentication.Models;
using ConvocadoFc.WebApi.Modules.Users.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConvocadoFc.WebApi.Modules.Users.Controllers;

/// <summary>
/// Endpoints de cadastro e criação de usuários.
/// </summary>
[ApiController]
[Route("api/users")]
public sealed class UsersController(
    IRegisterUserHandler registerUserHandler) : ControllerBase
{
    private readonly IRegisterUserHandler _registerUserHandler = registerUserHandler;

    /// <summary>
    /// Registra um novo usuário.
    /// Cria o perfil básico e atribui a role padrão.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _registerUserHandler.HandleAsync(new RegisterUserCommand(
            request.Name,
            request.Email,
            request.Phone,
            request.Password,
            request.Address,
            request.ProfilePhotoUrl),
            cancellationToken);

        if (result.Status == ERegisterUserStatus.EmailAlreadyExists)
        {
            return Conflict(new ApiResponse
            {
                StatusCode = StatusCodes.Status409Conflict,
                Success = false,
                Message = "E-mail já cadastrado."
            });
        }

        if (result.Status == ERegisterUserStatus.Failed)
        {
            return BadRequest(new ApiResponse
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Success = false,
                Message = "Operação não concluída.",
                Errors = result.Errors.ToList()
            });
        }

        if (result.User is null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Success = false,
                Message = "Falha ao processar o cadastro."
            });
        }

        return Ok(new ApiResponse<AuthUserResponse>
        {
            StatusCode = StatusCodes.Status200OK,
            Success = true,
            Message = "Cadastro realizado com sucesso. Confirme o e-mail para liberar ações críticas.",
            Data = new AuthUserResponse(
                result.User.Id,
                result.User.Email ?? string.Empty,
                result.User.FullName,
                result.User.EmailConfirmed,
                result.Roles)
        });
    }
}
