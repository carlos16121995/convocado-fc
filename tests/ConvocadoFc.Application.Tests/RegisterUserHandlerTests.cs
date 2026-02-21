using ConvocadoFc.Application.Handlers.Modules.Notifications.Interfaces;
using ConvocadoFc.Application.Handlers.Modules.Notifications.Models;
using ConvocadoFc.Application.Handlers.Modules.Shared.Interfaces;
using ConvocadoFc.Application.Handlers.Modules.Users.Implementations;
using ConvocadoFc.Application.Handlers.Modules.Users.Models;
using ConvocadoFc.Domain.Models.Modules.Notifications;
using ConvocadoFc.Domain.Models.Modules.Users.Identity;
using ConvocadoFc.Domain.Shared;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace ConvocadoFc.Application.Tests;

public sealed class RegisterUserHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenUserExists_ReturnsEmailAlreadyExists()
    {
        var userManager = CreateUserManager();
        var notificationService = new Mock<INotificationService>(MockBehavior.Strict);
        var appUrlProvider = new Mock<IAppUrlProvider>(MockBehavior.Strict);

        userManager.Setup(manager => manager.FindByEmailAsync("user@local"))
            .ReturnsAsync(new ApplicationUser { Id = Guid.NewGuid(), Email = "user@local" });

        var handler = new RegisterUserHandler(userManager.Object, notificationService.Object, appUrlProvider.Object);

        var result = await handler.HandleAsync(new RegisterUserCommand(
            "User",
            "user@local",
            "11999999999",
            "Password123!",
            null,
            null),
            CancellationToken.None);

        Assert.Equal(ERegisterUserStatus.EmailAlreadyExists, result.Status);
        Assert.Empty(result.Errors);
        Assert.Null(result.User);
        Assert.Empty(result.Roles);

        userManager.Verify(manager => manager.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        notificationService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_WhenCreateFails_ReturnsFailedAndErrors()
    {
        var userManager = CreateUserManager();
        var notificationService = new Mock<INotificationService>(MockBehavior.Strict);
        var appUrlProvider = new Mock<IAppUrlProvider>(MockBehavior.Strict);

        userManager.Setup(manager => manager.FindByEmailAsync("user@local"))
            .ReturnsAsync((ApplicationUser?)null);

        var error = new IdentityError { Code = "Password", Description = "Senha inválida." };
        userManager.Setup(manager => manager.CreateAsync(It.IsAny<ApplicationUser>(), "Password123!"))
            .ReturnsAsync(IdentityResult.Failed(error));

        var handler = new RegisterUserHandler(userManager.Object, notificationService.Object, appUrlProvider.Object);

        var result = await handler.HandleAsync(new RegisterUserCommand(
            "User",
            "user@local",
            "11999999999",
            "Password123!",
            null,
            null),
            CancellationToken.None);

        Assert.Equal(ERegisterUserStatus.Failed, result.Status);
        Assert.Single(result.Errors);
        Assert.Equal("Password", result.Errors.First().PropertyName);
        Assert.Null(result.User);
        Assert.Empty(result.Roles);

        userManager.Verify(manager => manager.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        notificationService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_WhenSuccessful_ReturnsUserAndSendsConfirmation()
    {
        var userManager = CreateUserManager();
        var notificationService = new Mock<INotificationService>(MockBehavior.Strict);
        var appUrlProvider = new Mock<IAppUrlProvider>(MockBehavior.Strict);

        appUrlProvider.SetupGet(provider => provider.ApiBaseUrl)
            .Returns("https://api.test");

        userManager.Setup(manager => manager.FindByEmailAsync("user@local"))
            .ReturnsAsync((ApplicationUser?)null);

        ApplicationUser? createdUser = null;
        userManager.Setup(manager => manager.CreateAsync(It.IsAny<ApplicationUser>(), "Password123!"))
            .Callback<ApplicationUser, string>((user, _) => createdUser = user)
            .ReturnsAsync(IdentityResult.Success);

        userManager.Setup(manager => manager.AddToRoleAsync(It.IsAny<ApplicationUser>(), SystemRoles.User))
            .ReturnsAsync(IdentityResult.Success);

        userManager.Setup(manager => manager.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync("token");

        userManager.Setup(manager => manager.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string> { SystemRoles.User });

        notificationService
            .Setup(service => service.SendAsync(It.IsAny<NotificationRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new RegisterUserHandler(userManager.Object, notificationService.Object, appUrlProvider.Object);

        var result = await handler.HandleAsync(new RegisterUserCommand(
            "User",
            "user@local",
            "11999999999",
            "Password123!",
            null,
            null),
            CancellationToken.None);

        Assert.Equal(ERegisterUserStatus.Success, result.Status);
        Assert.NotNull(result.User);
        Assert.NotNull(createdUser);
        Assert.Same(createdUser, result.User);
        Assert.Single(result.Roles);
        Assert.Equal(SystemRoles.User, result.Roles.First());

        notificationService.Verify(service => service.SendAsync(It.Is<NotificationRequest>(request =>
            request.Channel == ConvocadoFc.Domain.Models.Modules.Notifications.ENotificationChannel.Email
            && request.Reason == NotificationReasons.EmailConfirmation
            && request.To.Contains("user@local")
            && request.ActionUrl.Contains(createdUser!.Id.ToString())
            && request.ActionUrl.Contains("token=dG9rZW4")),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static Mock<UserManager<ApplicationUser>> CreateUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(
            store.Object,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!);
    }
}
