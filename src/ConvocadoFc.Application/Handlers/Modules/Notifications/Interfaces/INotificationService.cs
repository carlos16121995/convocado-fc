using System.Threading;
using System.Threading.Tasks;

using ConvocadoFc.Application.Handlers.Modules.Notifications.Models;

namespace ConvocadoFc.Application.Handlers.Modules.Notifications.Interfaces;

public interface INotificationService
{
    Task SendAsync(NotificationRequest request, CancellationToken cancellationToken = default);
}
