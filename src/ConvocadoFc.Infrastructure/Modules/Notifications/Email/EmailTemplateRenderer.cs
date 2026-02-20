using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using ConvocadoFc.Application.Handlers.Modules.Notifications.Interfaces;
using ConvocadoFc.Application.Handlers.Modules.Notifications.Models;
using Microsoft.Extensions.Options;

namespace ConvocadoFc.Infrastructure.Modules.Notifications.Email;

public sealed class EmailTemplateRenderer(IOptions<EmailSettings> options) : IEmailTemplateRenderer
{
    private readonly EmailSettings _settings = options.Value;

    public async Task<string> RenderAsync(EmailTemplateData data, CancellationToken cancellationToken = default)
    {
        var templatesRoot = Path.Combine(AppContext.BaseDirectory, _settings.TemplatesPath);
        var baseTemplatePath = Path.Combine(templatesRoot, _settings.BaseTemplateFile);
        var contentTemplatePath = Path.Combine(templatesRoot, _settings.ContentTemplateFile);

        var baseTemplate = await File.ReadAllTextAsync(baseTemplatePath, cancellationToken);
        var contentTemplate = await File.ReadAllTextAsync(contentTemplatePath, cancellationToken);

        var content = contentTemplate
            .Replace("{{Title}}", data.Title)
            .Replace("{{DynamicMessage}}", data.DynamicMessage)
            .Replace("{{ActionURL}}", data.ActionUrl);

        return baseTemplate.Replace("{{Content}}", content);
    }
}
