using ConvocadoFc.Application.Handlers.Modules.Notifications.Models;
using ConvocadoFc.Infrastructure.Modules.Notifications.Email;
using FluentEmail.Core;
using FluentEmail.Core.Models;
using Moq;

namespace ConvocadoFc.Infrastructure.Tests.Notifications;

public sealed class FluentEmailTransportTests
{
    [Fact]
    public async Task DeliverAsync_WhenSendFails_Throws()
    {
        var email = CreateFluentEmail(out var factory);
        email.Setup(e => e.SendAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SendResponse { ErrorMessages = new List<string> { "error" } });

        var transport = new FluentEmailTransport(factory.Object);

        var message = new EmailMessage("Subject", "<html/>", new List<string> { "user@local" }, null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => transport.DeliverAsync(message, CancellationToken.None));
    }

    [Fact]
    public async Task DeliverAsync_WhenSuccess_SendsEmail()
    {
        var email = CreateFluentEmail(out var factory);
        email.Setup(e => e.SendAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SendResponse());

        var transport = new FluentEmailTransport(factory.Object);

        var message = new EmailMessage("Subject", "<html/>", new List<string> { "user@local" }, new List<string> { "cc@local" });

        await transport.DeliverAsync(message, CancellationToken.None);

        email.Verify(e => e.To("user@local"), Times.Once);
        email.Verify(e => e.CC("cc@local"), Times.Once);
    }

    private static Mock<IFluentEmail> CreateFluentEmail(out Mock<IFluentEmailFactory> factory)
    {
        var email = new Mock<IFluentEmail>();
        email.Setup(e => e.Subject(It.IsAny<string>())).Returns(email.Object);
        email.Setup(e => e.Body(It.IsAny<string>(), true)).Returns(email.Object);
        email.Setup(e => e.To(It.IsAny<string>())).Returns(email.Object);
        email.Setup(e => e.CC(It.IsAny<string>())).Returns(email.Object);

        factory = new Mock<IFluentEmailFactory>();
        factory.Setup(f => f.Create()).Returns(email.Object);

        return email;
    }
}
