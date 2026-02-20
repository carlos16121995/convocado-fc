using ConvocadoFc.Application.Abstractions.Notifications.Models;

using System.Threading;
using System.Threading.Tasks;

namespace ConvocadoFc.Application.Abstractions.Notifications.Interfaces;

public interface IEmailTemplateRenderer
{
    Task<string> RenderAsync(EmailTemplateData data, CancellationToken cancellationToken = default);
}
