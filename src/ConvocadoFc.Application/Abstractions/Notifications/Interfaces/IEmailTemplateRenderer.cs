using ConvocadoFc.Application.Abstractions.Notifications.Models;

namespace ConvocadoFc.Application.Abstractions.Notifications.Interfaces;

public interface IEmailTemplateRenderer
{
    Task<string> RenderAsync(EmailTemplateData data, CancellationToken cancellationToken = default);
}
