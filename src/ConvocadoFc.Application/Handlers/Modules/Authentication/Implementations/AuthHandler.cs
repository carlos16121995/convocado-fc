using System.Text;

using ConvocadoFc.Application.Handlers.Modules.Authentication.Interfaces;
using ConvocadoFc.Application.Handlers.Modules.Authentication.Models;
using ConvocadoFc.Application.Handlers.Modules.Notifications.Interfaces;
using ConvocadoFc.Application.Handlers.Modules.Notifications.Models;
using ConvocadoFc.Application.Handlers.Modules.Shared.Interfaces;
using ConvocadoFc.Domain.Models.Modules.Notifications;
using ConvocadoFc.Domain.Models.Modules.Users.Identity;
using ConvocadoFc.Domain.Shared;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace ConvocadoFc.Application.Handlers.Modules.Authentication.Implementations;

public sealed class AuthHandler(
    UserManager<ApplicationUser> userManager,
    IJwtTokenService jwtTokenService,
    IRefreshTokenManager refreshTokenManager,
    INotificationService notificationService,
    IAppUrlProvider appUrlProvider) : IAuthHandler
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IJwtTokenService _jwtTokenService = jwtTokenService;
    private readonly IRefreshTokenManager _refreshTokenManager = refreshTokenManager;
    private readonly INotificationService _notificationService = notificationService;
    private readonly IAppUrlProvider _appUrlProvider = appUrlProvider;

    public async Task<AuthOperationResult> LoginAsync(LoginCommand command, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(command.Email);
        if (user is null)
        {
            return AuthOperationResult.Failure(EAuthOperationStatus.InvalidCredentials);
        }

        var isValid = await _userManager.CheckPasswordAsync(user, command.Password);
        if (!isValid)
        {
            return AuthOperationResult.Failure(EAuthOperationStatus.InvalidCredentials);
        }

        return await BuildAuthSuccessAsync(user, cancellationToken);
    }

    public async Task<AuthOperationResult> GoogleLoginAsync(GoogleLoginCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Email))
        {
            return AuthOperationResult.Failure(EAuthOperationStatus.InvalidData);
        }

        var user = await _userManager.FindByEmailAsync(command.Email);
        if (user is null)
        {
            if (string.IsNullOrWhiteSpace(command.Phone))
            {
                return AuthOperationResult.Failure(EAuthOperationStatus.RequiresPhone);
            }

            user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = command.Email,
                Email = command.Email,
                FullName = string.IsNullOrWhiteSpace(command.FullName) ? command.Email : command.FullName,
                PhoneNumber = command.Phone,
                EmailConfirmed = command.EmailVerified
            };

            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                return AuthOperationResult.Failure(EAuthOperationStatus.Failed, ToValidationFailures(result));
            }

            await _userManager.AddToRoleAsync(user, SystemRoles.User);
        }
        else if (command.EmailVerified && !user.EmailConfirmed)
        {
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);
        }

        return await BuildAuthSuccessAsync(user, cancellationToken);
    }

    public async Task<AuthOperationResult> RefreshTokenAsync(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.RefreshToken))
        {
            return AuthOperationResult.Failure(EAuthOperationStatus.RefreshTokenMissing);
        }

        var descriptor = await _refreshTokenManager.ValidateAsync(command.RefreshToken, cancellationToken);
        if (descriptor is null)
        {
            return AuthOperationResult.Failure(EAuthOperationStatus.RefreshTokenInvalid);
        }

        var user = await _userManager.FindByIdAsync(descriptor.UserId.ToString());
        if (user is null || descriptor.SecurityStamp != (user.SecurityStamp ?? string.Empty))
        {
            return AuthOperationResult.Failure(EAuthOperationStatus.RefreshTokenInvalid);
        }

        var newRefreshToken = await _refreshTokenManager.RotateAsync(command.RefreshToken, user, cancellationToken);
        if (string.IsNullOrWhiteSpace(newRefreshToken))
        {
            return AuthOperationResult.Failure(EAuthOperationStatus.RefreshTokenInvalid);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var jwt = _jwtTokenService.CreateToken(user, roles);
        var authUser = new AuthUserDto(user.Id, user.Email ?? string.Empty, user.FullName, user.EmailConfirmed, roles.ToArray());

        return AuthOperationResult.Success(authUser, jwt, newRefreshToken);
    }

    public async Task<AuthOperationResult> RevokeRefreshTokenAsync(RevokeRefreshTokenCommand command, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(command.RefreshToken))
        {
            await _refreshTokenManager.RevokeAsync(command.RefreshToken, cancellationToken);
        }

        return AuthOperationResult.Success();
    }

    public async Task<AuthOperationResult> ChangePasswordAsync(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(command.UserId.ToString());
        if (user is null)
        {
            return AuthOperationResult.Failure(EAuthOperationStatus.UserNotFound);
        }

        var result = await _userManager.ChangePasswordAsync(user, command.CurrentPassword, command.NewPassword);
        if (!result.Succeeded)
        {
            return AuthOperationResult.Failure(EAuthOperationStatus.Failed, ToValidationFailures(result));
        }

        return AuthOperationResult.Success();
    }

    public async Task<AuthOperationResult> ForgotPasswordAsync(ForgotPasswordCommand command, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(command.Email);
        if (user is null)
        {
            return AuthOperationResult.Success();
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var resetUrl = BuildWebUrl(_appUrlProvider.WebBaseUrl, "reset-password", user.Id, encodedToken);

        await _notificationService.SendAsync(new NotificationRequest(
            ENotificationChannel.Email,
            NotificationReasons.PasswordReset,
            "Recuperação de senha",
            "Recebemos uma solicitação para redefinir sua senha. Caso não tenha sido você, ignore este e-mail.",
            resetUrl,
            new[] { user.Email! }),
            cancellationToken);

        return AuthOperationResult.Success();
    }

    public async Task<AuthOperationResult> ResetPasswordAsync(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(command.UserId.ToString());
        if (user is null)
        {
            return AuthOperationResult.Failure(EAuthOperationStatus.UserNotFound);
        }

        var result = await _userManager.ResetPasswordAsync(user, command.Token, command.NewPassword);
        if (!result.Succeeded)
        {
            return AuthOperationResult.Failure(EAuthOperationStatus.Failed, ToValidationFailures(result));
        }

        return AuthOperationResult.Success();
    }

    public async Task<AuthOperationResult> ConfirmEmailAsync(ConfirmEmailCommand command, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(command.UserId.ToString());
        if (user is null)
        {
            return AuthOperationResult.Failure(EAuthOperationStatus.UserNotFound);
        }

        var result = await _userManager.ConfirmEmailAsync(user, command.Token);
        if (!result.Succeeded)
        {
            return AuthOperationResult.Failure(EAuthOperationStatus.Failed, ToValidationFailures(result));
        }

        return AuthOperationResult.Success();
    }

    private async Task<AuthOperationResult> BuildAuthSuccessAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var refreshToken = await _refreshTokenManager.CreateAsync(user, cancellationToken);
        var roles = await _userManager.GetRolesAsync(user);
        var jwt = _jwtTokenService.CreateToken(user, roles);
        var authUser = new AuthUserDto(user.Id, user.Email ?? string.Empty, user.FullName, user.EmailConfirmed, roles.ToArray());

        return AuthOperationResult.Success(authUser, jwt, refreshToken);
    }

    private static IReadOnlyCollection<ValidationFailure> ToValidationFailures(IdentityResult result)
        => result.Errors.Select(error => new ValidationFailure
        {
            PropertyName = error.Code,
            ErrorMessage = error.Description
        }).ToList();

    private static string BuildWebUrl(string baseUrl, string path, Guid userId, string token)
        => $"{baseUrl.TrimEnd('/')}/{path}?userId={userId}&token={token}";
}
