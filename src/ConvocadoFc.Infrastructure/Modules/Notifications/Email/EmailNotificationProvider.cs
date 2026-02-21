using ConvocadoFc.Application.Handlers.Modules.Notifications.Interfaces;
using ConvocadoFc.Application.Handlers.Modules.Notifications.Models;
using ConvocadoFc.Domain.Models.Modules.Notifications;

namespace ConvocadoFc.Infrastructure.Modules.Notifications.Email;

public sealed class EmailNotificationProvider(
    IMessageTransport<EmailMessage> transport,
    IEmailTemplateRenderer templateRenderer) : INotificationProvider
{
    private readonly IMessageTransport<EmailMessage> _transport = transport;
    private readonly IEmailTemplateRenderer _templateRenderer = templateRenderer;

    public ENotificationChannel Channel => ENotificationChannel.Email;

    public async Task SendAsync(NotificationRequest request, CancellationToken cancellationToken = default)
    {
        if (request.To is null || request.To.Count == 0)
        {
            throw new ArgumentException("Lista de destinatários não pode ser vazia.", nameof(request));
        }

        var htmlBody = await _templateRenderer.RenderAsync(
            new EmailTemplateData(request.Title, request.Message, request.ActionUrl),
            cancellationToken);

        var message = new EmailMessage(request.Title, htmlBody, request.To, request.Cc);

        await _transport.DeliverAsync(message, cancellationToken);
    }
}
