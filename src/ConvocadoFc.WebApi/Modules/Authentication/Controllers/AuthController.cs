using System.Text;

using ConvocadoFc.Application.Handlers.Modules.Authentication.Interfaces;
using ConvocadoFc.Application.Handlers.Modules.Authentication.Models;
using ConvocadoFc.Domain.Shared;
using ConvocadoFc.Infrastructure.Modules.Authentication;
using ConvocadoFc.WebApi.Modules.Authentication.Models;
using ConvocadoFc.WebApi.Options;
using ConvocadoFc.WebApi.Extensions;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace ConvocadoFc.WebApi.Modules.Authentication.Controllers;

/// <summary>
/// Endpoints de autenticação, sessão e recuperação de acesso.
/// </summary>
[ApiController]
[Route("api")]
public sealed class AuthController(
    IAuthHandler authHandler,
    IOptions<JwtOptions> jwtOptions,
    IOptions<RefreshTokenOptions> refreshTokenOptions,
    IOptions<AuthCookieOptions> authCookieOptions,
    IOptions<GoogleAuthOptions> googleAuthOptions) : ControllerBase
{
    private readonly IAuthHandler _authHandler = authHandler;
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;
    private readonly RefreshTokenOptions _refreshTokenOptions = refreshTokenOptions.Value;
    private readonly AuthCookieOptions _cookieOptions = authCookieOptions.Value;
    private readonly GoogleAuthOptions _googleAuthOptions = googleAuthOptions.Value;

    /// <summary>
    /// Autentica com e-mail e senha e cria uma sessão.
    /// Emite access e refresh tokens em cookies HTTP-only.
    /// </summary>
    [HttpPost("sessions")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authHandler.LoginAsync(new LoginCommand(request.Email, request.Password), cancellationToken);
        if (result.Status == EAuthOperationStatus.InvalidCredentials)
        {
            return Unauthorized(new ApiResponse
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Success = false,
                Message = "Credenciais inválidas."
            });
        }

        if (result.Status != EAuthOperationStatus.Success)
        {
            return BadRequest(ToApiResponse(result.Errors));
        }

        WriteAuthCookies(result);

        return Ok(new ApiResponse<AuthUserResponse>
        {
            StatusCode = StatusCodes.Status200OK,
            Success = true,
            Message = "Autenticação realizada com sucesso.",
            Data = MapToResponse(result.User!)
        });
    }

    /// <summary>
    /// Autentica via Google e cria uma sessão.
    /// Se o usuário não existir, realiza o cadastro básico.
    /// </summary>
    [HttpPost("sessions/google")]
    [AllowAnonymous]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_googleAuthOptions.ClientId))
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Success = false,
                Message = "Google ClientId não configurado."
            });
        }

        cancellationToken.ThrowIfCancellationRequested();
        var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = new[] { _googleAuthOptions.ClientId }
        });

        if (string.IsNullOrWhiteSpace(payload.Email))
        {
            return BadRequest(new ApiResponse
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Success = false,
                Message = "E-mail inválido no token do Google."
            });
        }

        var result = await _authHandler.GoogleLoginAsync(new GoogleLoginCommand(
            payload.Email,
            payload.Name,
            request.Phone,
            payload.EmailVerified),
            cancellationToken);

        if (result.Status == EAuthOperationStatus.RequiresPhone)
        {
            return BadRequest(new ApiResponse
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Success = false,
                Message = "Telefone é obrigatório para concluir o cadastro via Google."
            });
        }

        if (result.Status != EAuthOperationStatus.Success)
        {
            return BadRequest(ToApiResponse(result.Errors));
        }

        WriteAuthCookies(result);

        return Ok(new ApiResponse<AuthUserResponse>
        {
            StatusCode = StatusCodes.Status200OK,
            Success = true,
            Message = "Autenticação via Google realizada com sucesso.",
            Data = MapToResponse(result.User!)
        });
    }

    /// <summary>
    /// Atualiza o token de acesso usando o refresh token do cookie.
    /// Também rotaciona o refresh token por segurança.
    /// </summary>
    [HttpPost("tokens/refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies[_refreshTokenOptions.CookieName];
        var result = await _authHandler.RefreshTokenAsync(new RefreshTokenCommand(refreshToken), cancellationToken);

        if (result.Status is EAuthOperationStatus.RefreshTokenMissing or EAuthOperationStatus.RefreshTokenInvalid)
        {
            return Unauthorized(new ApiResponse
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Success = false,
                Message = string.IsNullOrWhiteSpace(refreshToken) ? "Refresh token ausente." : "Refresh token inválido."
            });
        }

        if (result.Status != EAuthOperationStatus.Success)
        {
            return BadRequest(ToApiResponse(result.Errors));
        }

        WriteAuthCookies(result);

        return Ok(new ApiResponse<AuthUserResponse>
        {
            StatusCode = StatusCodes.Status200OK,
            Success = true,
            Message = "Token atualizado com sucesso.",
            Data = MapToResponse(result.User!)
        });
    }

    /// <summary>
    /// Encerra a sessão atual e revoga o refresh token.
    /// Remove os cookies de autenticação.
    /// </summary>
    [HttpDelete("sessions/current")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies[_refreshTokenOptions.CookieName];
        await _authHandler.RevokeRefreshTokenAsync(new RevokeRefreshTokenCommand(refreshToken), cancellationToken);

        DeleteAuthCookies();

        return Ok(new ApiResponse
        {
            StatusCode = StatusCodes.Status200OK,
            Success = true,
            Message = "Logout realizado com sucesso."
        });
    }

    /// <summary>
    /// Altera a senha do usuário autenticado.
    /// Requer a senha atual para validação.
    /// </summary>
    [HttpPatch("users/me/password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized(new ApiResponse
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Success = false,
                Message = "Usuário não autenticado."
            });
        }

        var result = await _authHandler.ChangePasswordAsync(new ChangePasswordCommand(
            userId,
            request.CurrentPassword,
            request.NewPassword),
            cancellationToken);

        if (result.Status != EAuthOperationStatus.Success)
        {
            return BadRequest(ToApiResponse(result.Errors));
        }

        return Ok(new ApiResponse
        {
            StatusCode = StatusCodes.Status200OK,
            Success = true,
            Message = "Senha atualizada com sucesso."
        });
    }

    /// <summary>
    /// Solicita recuperação de senha por e-mail.
    /// Envia um link com token de redefinição quando o e-mail existe.
    /// </summary>
    [HttpPost("password-resets")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        await _authHandler.ForgotPasswordAsync(new ForgotPasswordCommand(request.Email), cancellationToken);

        return Ok(new ApiResponse
        {
            StatusCode = StatusCodes.Status200OK,
            Success = true,
            Message = "Se o e-mail existir, enviaremos instruções para redefinir a senha."
        });
    }

    /// <summary>
    /// Redefine a senha usando o token de recuperação.
    /// Valida o usuário e o token informado.
    /// </summary>
    [HttpPut("password-resets/{token}")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromRoute] string token, [FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        var result = await _authHandler.ResetPasswordAsync(new ResetPasswordCommand(
            request.UserId,
            decodedToken,
            request.NewPassword),
            cancellationToken);

        if (result.Status == EAuthOperationStatus.UserNotFound)
        {
            return NotFound(new ApiResponse
            {
                StatusCode = StatusCodes.Status404NotFound,
                Success = false,
                Message = "Usuário não encontrado."
            });
        }

        if (result.Status != EAuthOperationStatus.Success)
        {
            return BadRequest(ToApiResponse(result.Errors));
        }

        return Ok(new ApiResponse
        {
            StatusCode = StatusCodes.Status200OK,
            Success = true,
            Message = "Senha redefinida com sucesso."
        });
    }

    /// <summary>
    /// Confirma o e-mail do usuário com token.
    /// Libera ações que exigem e-mail confirmado.
    /// </summary>
    [HttpPut("users/{userId:guid}/email-confirmation")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail([FromRoute] Guid userId, [FromQuery] string token, CancellationToken cancellationToken)
    {
        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        var result = await _authHandler.ConfirmEmailAsync(new ConfirmEmailCommand(userId, decodedToken), cancellationToken);

        if (result.Status == EAuthOperationStatus.UserNotFound)
        {
            return NotFound(new ApiResponse
            {
                StatusCode = StatusCodes.Status404NotFound,
                Success = false,
                Message = "Usuário não encontrado."
            });
        }

        if (result.Status != EAuthOperationStatus.Success)
        {
            return BadRequest(ToApiResponse(result.Errors));
        }

        return Ok(new ApiResponse
        {
            StatusCode = StatusCodes.Status200OK,
            Success = true,
            Message = "E-mail confirmado com sucesso."
        });
    }

    private void WriteAuthCookies(AuthOperationResult result)
    {
        WriteCookie(_jwtOptions.CookieName, result.AccessToken!, DateTimeOffset.UtcNow.AddMinutes(_jwtOptions.AccessTokenMinutes));
        WriteCookie(_refreshTokenOptions.CookieName, result.RefreshToken!, DateTimeOffset.UtcNow.AddDays(_refreshTokenOptions.ExpirationDays));
    }

    private void WriteCookie(string name, string value, DateTimeOffset expiresAt) => Response.Cookies.Append(name, value, new CookieOptions
    {
        HttpOnly = true,
        Secure = _cookieOptions.Secure,
        SameSite = _cookieOptions.SameSite,
        Expires = expiresAt.UtcDateTime,
        Path = _cookieOptions.Path,
        Domain = string.IsNullOrWhiteSpace(_cookieOptions.Domain) ? null : _cookieOptions.Domain
    });

    private void DeleteAuthCookies()
    {
        Response.Cookies.Delete(_jwtOptions.CookieName, new CookieOptions
        {
            Path = _cookieOptions.Path,
            Domain = string.IsNullOrWhiteSpace(_cookieOptions.Domain) ? null : _cookieOptions.Domain
        });

        Response.Cookies.Delete(_refreshTokenOptions.CookieName, new CookieOptions
        {
            Path = _cookieOptions.Path,
            Domain = string.IsNullOrWhiteSpace(_cookieOptions.Domain) ? null : _cookieOptions.Domain
        });
    }

    private static AuthUserResponse MapToResponse(AuthUserDto user)
        => new(user.UserId, user.Email, user.FullName, user.EmailConfirmed, user.Roles.ToArray());

    private static ApiResponse ToApiResponse(IReadOnlyCollection<ValidationFailure> errors) => new()
    {
        StatusCode = StatusCodes.Status400BadRequest,
        Success = false,
        Message = "Operação não concluída.",
        Errors = errors.ToList()
    };
}
