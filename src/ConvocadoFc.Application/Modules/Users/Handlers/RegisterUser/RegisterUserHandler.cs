using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using ConvocadoFc.Application.Abstractions.Notifications.Interfaces;
using ConvocadoFc.Application.Abstractions.Notifications.Models;
using ConvocadoFc.Application.Abstractions.AppUrls;
using ConvocadoFc.Domain.Modules.Users.Identity;
using ConvocadoFc.Domain.Notifications;
using ConvocadoFc.Domain.Shared;
using Microsoft.AspNetCore.Identity;

namespace ConvocadoFc.Application.Modules.Users.Handlers.RegisterUser;

public sealed class RegisterUserHandler(
    UserManager<ApplicationUser> userManager,
    INotificationService notificationService,
    IAppUrlProvider appUrlProvider)
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly INotificationService _notificationService = notificationService;
    private readonly IAppUrlProvider _appUrlProvider = appUrlProvider;

    public async Task<RegisterUserResult> HandleAsync(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var existingUser = await _userManager.FindByEmailAsync(command.Email);
        if (existingUser is not null)
        {
            return new RegisterUserResult(RegisterUserStatus.EmailAlreadyExists, Array.Empty<ValidationFailure>(), null, Array.Empty<string>());
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = command.Email,
            Email = command.Email,
            FullName = command.Name,
            PhoneNumber = command.Phone,
            Address = command.Address,
            ProfilePhotoUrl = command.ProfilePhotoUrl
        };

        var result = await _userManager.CreateAsync(user, command.Password);
        if (!result.Succeeded)
        {
            return new RegisterUserResult(RegisterUserStatus.Failed, ToValidationFailures(result), null, Array.Empty<string>());
        }

        await _userManager.AddToRoleAsync(user, SystemRoles.User);

        await SendEmailConfirmationAsync(user, cancellationToken);

        var roles = await _userManager.GetRolesAsync(user);

        return new RegisterUserResult(RegisterUserStatus.Success, Array.Empty<ValidationFailure>(), user, roles.ToArray());
    }

    private async Task SendEmailConfirmationAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var confirmUrl = BuildApiUrl(_appUrlProvider.ApiBaseUrl, "confirm-email", user.Id, encodedToken);

        await _notificationService.SendAsync(new NotificationRequest(
            NotificationChannel.Email,
            NotificationReasons.EmailConfirmation,
            "Confirme seu e-mail",
            "Clique no botão abaixo para confirmar seu e-mail e liberar ações críticas.",
            confirmUrl,
            new[] { user.Email! }),
            cancellationToken);
    }

    private static string BuildApiUrl(string baseUrl, string path, Guid userId, string token)
        => $"{baseUrl.TrimEnd('/')}/api/auth/{path}?userId={userId}&token={token}";

    private static IReadOnlyCollection<ValidationFailure> ToValidationFailures(IdentityResult result)
        => result.Errors.Select(error => new ValidationFailure
        {
            PropertyName = error.Code,
            ErrorMessage = error.Description
        }).ToList();

    private static string Base64UrlEncode(byte[] input)
        => Convert.ToBase64String(input).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
