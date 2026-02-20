using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using ConvocadoFc.Application.Abstractions.Authentication;
using ConvocadoFc.Application.Abstractions.Notifications.Interfaces;
using ConvocadoFc.Application.Abstractions.Notifications.Models;
using ConvocadoFc.Domain.Identity;
using ConvocadoFc.Domain.Notifications;
using ConvocadoFc.Domain.Shared;
using ConvocadoFc.Infrastructure.Authentication;
using ConvocadoFc.WebApi.Models.Auth;
using ConvocadoFc.WebApi.Options;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace ConvocadoFc.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(
    UserManager<ApplicationUser> userManager,
    IJwtTokenService jwtTokenService,
    IRefreshTokenManager refreshTokenManager,
    INotificationService notificationService,
    IOptions<JwtOptions> jwtOptions,
    IOptions<RefreshTokenOptions> refreshTokenOptions,
    IOptions<AuthCookieOptions> authCookieOptions,
    IOptions<AppUrlOptions> appUrlOptions,
    IOptions<GoogleAuthOptions> googleAuthOptions) : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IJwtTokenService _jwtTokenService = jwtTokenService;
    private readonly IRefreshTokenManager _refreshTokenManager = refreshTokenManager;
    private readonly INotificationService _notificationService = notificationService;
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;
    private readonly RefreshTokenOptions _refreshTokenOptions = refreshTokenOptions.Value;
    private readonly AuthCookieOptions _cookieOptions = authCookieOptions.Value;
    private readonly AppUrlOptions _appUrlOptions = appUrlOptions.Value;
    private readonly GoogleAuthOptions _googleAuthOptions = googleAuthOptions.Value;

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            return Conflict(new ApiResponse
            {
                StatusCode = StatusCodes.Status409Conflict,
                Success = false,
                Message = "E-mail já cadastrado."
            });
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = request.Email,
            Email = request.Email,
            FullName = request.Name,
            PhoneNumber = request.Phone,
            Address = request.Address,
            ProfilePhotoUrl = request.ProfilePhotoUrl
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return BadRequest(ToApiResponse(result));
        }

        await _userManager.AddToRoleAsync(user, SystemRoles.User);

        await SendEmailConfirmationAsync(user, cancellationToken);

        return Ok(new ApiResponse<AuthUserResponse>
        {
            StatusCode = StatusCodes.Status200OK,
            Success = true,
            Message = "Cadastro realizado com sucesso. Confirme o e-mail para liberar ações críticas.",
            Data = await BuildAuthUserResponseAsync(user, cancellationToken)
        });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Unauthorized(new ApiResponse
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Success = false,
                Message = "Credenciais inválidas."
            });
        }

        var isValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isValid)
        {
            return Unauthorized(new ApiResponse
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Success = false,
                Message = "Credenciais inválidas."
            });
        }

        await IssueTokensAsync(user, cancellationToken);

        return Ok(new ApiResponse<AuthUserResponse>
        {
            StatusCode = StatusCodes.Status200OK,
            Success = true,
            Message = "Autenticação realizada com sucesso.",
            Data = await BuildAuthUserResponseAsync(user, cancellationToken)
        });
    }

    [HttpPost("google")]
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

        var user = await _userManager.FindByEmailAsync(payload.Email);
        if (user is null)
        {
            if (string.IsNullOrWhiteSpace(request.Phone))
            {
                return BadRequest(new ApiResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Success = false,
                    Message = "Telefone é obrigatório para concluir o cadastro via Google."
                });
            }

            user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = payload.Email,
                Email = payload.Email,
                FullName = payload.Name ?? payload.Email,
                PhoneNumber = request.Phone,
                EmailConfirmed = payload.EmailVerified
            };

            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(ToApiResponse(result));
            }

            await _userManager.AddToRoleAsync(user, SystemRoles.User);
        }
        else if (payload.EmailVerified && !user.EmailConfirmed)
        {
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);
        }

        await IssueTokensAsync(user, cancellationToken);

        return Ok(new ApiResponse<AuthUserResponse>
        {
            StatusCode = StatusCodes.Status200OK,
            Success = true,
            Message = "Autenticação via Google realizada com sucesso.",
            Data = await BuildAuthUserResponseAsync(user, cancellationToken)
        });
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies[_refreshTokenOptions.CookieName];
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Unauthorized(new ApiResponse
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Success = false,
                Message = "Refresh token ausente."
            });
        }

        var descriptor = await _refreshTokenManager.ValidateAsync(refreshToken, cancellationToken);
        if (descriptor is null)
        {
            return Unauthorized(new ApiResponse
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Success = false,
                Message = "Refresh token inválido."
            });
        }

        var user = await _userManager.FindByIdAsync(descriptor.UserId.ToString());
        if (user is null || descriptor.SecurityStamp != (user.SecurityStamp ?? string.Empty))
        {
            return Unauthorized(new ApiResponse
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Success = false,
                Message = "Refresh token inválido."
            });
        }

        var newRefreshToken = await _refreshTokenManager.RotateAsync(refreshToken, user, cancellationToken);
        if (string.IsNullOrWhiteSpace(newRefreshToken))
        {
            return Unauthorized(new ApiResponse
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Success = false,
                Message = "Refresh token inválido."
            });
        }

        await IssueTokensAsync(user, newRefreshToken, cancellationToken);

        return Ok(new ApiResponse<AuthUserResponse>
        {
            StatusCode = StatusCodes.Status200OK,
            Success = true,
            Message = "Token atualizado com sucesso.",
            Data = await BuildAuthUserResponseAsync(user, cancellationToken)
        });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies[_refreshTokenOptions.CookieName];
        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            await _refreshTokenManager.RevokeAsync(refreshToken, cancellationToken);
        }

        DeleteAuthCookies();

        return Ok(new ApiResponse
        {
            StatusCode = StatusCodes.Status200OK,
            Success = true,
            Message = "Logout realizado com sucesso."
        });
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized(new ApiResponse
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Success = false,
                Message = "Usuário não autenticado."
            });
        }

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            return BadRequest(ToApiResponse(result));
        }

        return Ok(new ApiResponse
        {
            StatusCode = StatusCodes.Status200OK,
            Success = true,
            Message = "Senha atualizada com sucesso."
        });
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is not null)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var resetUrl = BuildWebUrl(_appUrlOptions.WebBaseUrl, "reset-password", user.Id, encodedToken);

            await _notificationService.SendAsync(new NotificationRequest(
                NotificationChannel.Email,
                NotificationReasons.PasswordReset,
                "Recuperação de senha",
                "Recebemos uma solicitação para redefinir sua senha. Caso não tenha sido você, ignore este e-mail.",
                resetUrl,
                new[] { user.Email! }),
                cancellationToken);
        }

        return Ok(new ApiResponse
        {
            StatusCode = StatusCodes.Status200OK,
            Success = true,
            Message = "Se o e-mail existir, enviaremos instruções para redefinir a senha."
        });
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null)
        {
            return NotFound(new ApiResponse
            {
                StatusCode = StatusCodes.Status404NotFound,
                Success = false,
                Message = "Usuário não encontrado."
            });
        }

        var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
        var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);
        if (!result.Succeeded)
        {
            return BadRequest(ToApiResponse(result));
        }

        return Ok(new ApiResponse
        {
            StatusCode = StatusCodes.Status200OK,
            Success = true,
            Message = "Senha redefinida com sucesso."
        });
    }

    [HttpGet("confirm-email")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail([FromQuery] Guid userId, [FromQuery] string token, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
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

        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
        if (!result.Succeeded)
        {
            return BadRequest(ToApiResponse(result));
        }

        return Ok(new ApiResponse
        {
            StatusCode = StatusCodes.Status200OK,
            Success = true,
            Message = "E-mail confirmado com sucesso."
        });
    }

    private async Task IssueTokensAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var refreshToken = await _refreshTokenManager.CreateAsync(user, cancellationToken);
        await IssueTokensAsync(user, refreshToken, cancellationToken);
    }

    private async Task IssueTokensAsync(ApplicationUser user, string refreshToken, CancellationToken cancellationToken)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var jwt = _jwtTokenService.CreateToken(user, roles);

        WriteCookie(_jwtOptions.CookieName, jwt, DateTimeOffset.UtcNow.AddMinutes(_jwtOptions.AccessTokenMinutes));
        WriteCookie(_refreshTokenOptions.CookieName, refreshToken, DateTimeOffset.UtcNow.AddDays(_refreshTokenOptions.ExpirationDays));
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

    private async Task SendEmailConfirmationAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var confirmUrl = BuildApiUrl(_appUrlOptions.ApiBaseUrl, "confirm-email", user.Id, encodedToken);

        await _notificationService.SendAsync(new NotificationRequest(
            NotificationChannel.Email,
            NotificationReasons.EmailConfirmation,
            "Confirme seu e-mail",
            "Clique no botão abaixo para confirmar seu e-mail e liberar ações críticas.",
            confirmUrl,
            new[] { user.Email! }),
            cancellationToken);
    }

    private string BuildApiUrl(string baseUrl, string path, Guid userId, string token)
        => $"{baseUrl.TrimEnd('/')}/api/auth/{path}?userId={userId}&token={token}";

    private string BuildWebUrl(string baseUrl, string path, Guid userId, string token)
        => $"{baseUrl.TrimEnd('/')}/{path}?userId={userId}&token={token}";

    private async Task<AuthUserResponse> BuildAuthUserResponseAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var roles = await _userManager.GetRolesAsync(user);
        return new AuthUserResponse(user.Id, user.Email ?? string.Empty, user.FullName, user.EmailConfirmed, roles.ToArray());
    }

    private static ApiResponse ToApiResponse(IdentityResult result) => new()
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
