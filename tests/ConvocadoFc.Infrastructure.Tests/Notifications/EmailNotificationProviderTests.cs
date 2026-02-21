using ConvocadoFc.Application.Handlers.Modules.Notifications.Interfaces;
using ConvocadoFc.Application.Handlers.Modules.Notifications.Models;
using ConvocadoFc.Domain.Models.Modules.Notifications;
using ConvocadoFc.Infrastructure.Modules.Notifications.Email;
using Moq;

namespace ConvocadoFc.Infrastructure.Tests.Notifications;

public sealed class EmailNotificationProviderTests
{
    [Fact]
    public async Task SendAsync_WhenNoRecipients_Throws()
    {
        var transport = new Mock<IMessageTransport<EmailMessage>>(MockBehavior.Strict);
        var renderer = new Mock<IEmailTemplateRenderer>(MockBehavior.Strict);
        var provider = new EmailNotificationProvider(transport.Object, renderer.Object);

        var request = new NotificationRequest(
            ENotificationChannel.Email,
            NotificationReasons.EmailConfirmation,
            "Title",
            "Message",
            "https://test",
            new List<string>(),
            null,
            Guid.Empty,
            Guid.Empty);

        await Assert.ThrowsAsync<ArgumentException>(() => provider.SendAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task SendAsync_WhenValid_SendsEmail()
    {
        var transport = new Mock<IMessageTransport<EmailMessage>>(MockBehavior.Strict);
        var renderer = new Mock<IEmailTemplateRenderer>(MockBehavior.Strict);
        renderer.Setup(r => r.RenderAsync(It.IsAny<EmailTemplateData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("<html>body</html>");

        transport.Setup(t => t.DeliverAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var provider = new EmailNotificationProvider(transport.Object, renderer.Object);

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

        await provider.SendAsync(request, CancellationToken.None);

        transport.Verify(t => t.DeliverAsync(It.Is<EmailMessage>(msg => msg.Subject == "Title"), It.IsAny<CancellationToken>()), Times.Once);
    }
}
