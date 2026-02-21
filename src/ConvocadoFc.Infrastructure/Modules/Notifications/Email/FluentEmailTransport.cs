using ConvocadoFc.Application.Handlers.Modules.Notifications.Interfaces;
using ConvocadoFc.Application.Handlers.Modules.Notifications.Models;

using FluentEmail.Core;

namespace ConvocadoFc.Infrastructure.Modules.Notifications.Email;

public sealed class FluentEmailTransport(IFluentEmailFactory emailFactory) : IMessageTransport<EmailMessage>
{
    private readonly IFluentEmailFactory _emailFactory = emailFactory;

    public async Task DeliverAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        var email = _emailFactory
            .Create()
            .Subject(message.Subject)
            .Body(message.HtmlBody, isHtml: true);

        foreach (var to in message.To)
        {
            email.To(to);
        }

        if (message.Cc is { Count: > 0 })
        {
            foreach (var cc in message.Cc)
            {
                email.CC(cc);
            }
        }

        var result = await email.SendAsync(cancellationToken);

        if (!result.Successful)
        {
            var errors = string.Join("; ", result.ErrorMessages ?? Array.Empty<string>());
            throw new InvalidOperationException($"Falha ao enviar email: {errors}");
        }
    }
}
