using ConvocadoFc.Application.Handlers.Modules.Notifications.Interfaces;
using ConvocadoFc.Application.Handlers.Modules.Notifications.Models;
using ConvocadoFc.Domain.Models.Modules.Notifications;
using ConvocadoFc.Infrastructure.Modules.Notifications;
using ConvocadoFc.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;

namespace ConvocadoFc.Infrastructure.Tests.Notifications;

public sealed class NotificationServiceTests
{
    [Fact]
    public async Task SendAsync_WhenProviderMissing_Throws()
    {
        await using var context = CreateContext();
        var service = new NotificationService(Array.Empty<INotificationProvider>(), context);

        var request = new NotificationRequest(
            ENotificationChannel.Email,
            NotificationReasons.EmailConfirmation,
            "Title",
            "Message",
            "https://test",
            new List<string> { "user@local" },
            null,
            Guid.Empty,
            Guid.Empty);

        await Assert.ThrowsAsync<NotSupportedException>(() => service.SendAsync(request, CancellationToken.None));
        Assert.Empty(context.NotificationLogs);
    }

    [Fact]
    public async Task SendAsync_WhenSuccess_LogsNotification()
    {
        await using var context = CreateContext();
        var provider = new Mock<INotificationProvider>(MockBehavior.Strict);
        provider.SetupGet(p => p.Channel).Returns(ENotificationChannel.Email);
        provider.Setup(p => p.SendAsync(It.IsAny<NotificationRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new NotificationService(new[] { provider.Object }, context);
        var request = new NotificationRequest(
            ENotificationChannel.Email,
            NotificationReasons.EmailConfirmation,
            "Title",
            "Message",
            "https://test",
            new List<string> { "user@local" },
            null,
            Guid.Empty,
            Guid.Empty);

        await service.SendAsync(request, CancellationToken.None);

        var log = await context.NotificationLogs.FirstAsync();
        Assert.True(log.IsSuccess);
        Assert.Equal(ENotificationChannel.Email, log.Channel);
    }

    [Fact]
    public async Task SendAsync_WhenProviderFails_LogsAndThrows()
    {
        await using var context = CreateContext();
        var provider = new Mock<INotificationProvider>(MockBehavior.Strict);
        provider.SetupGet(p => p.Channel).Returns(ENotificationChannel.Email);
        provider.Setup(p => p.SendAsync(It.IsAny<NotificationRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("fail"));

        var service = new NotificationService(new[] { provider.Object }, context);
        var request = new NotificationRequest(
            ENotificationChannel.Email,
            NotificationReasons.EmailConfirmation,
            "Title",
            "Message",
            "https://test",
            new List<string> { "user@local" },
            null,
            Guid.Empty,
            Guid.Empty);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.SendAsync(request, CancellationToken.None));

        var log = await context.NotificationLogs.FirstAsync();
        Assert.False(log.IsSuccess);
        Assert.Equal("fail", log.ErrorMessage);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new AppDbContext(options);
    }
}
