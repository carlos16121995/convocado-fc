using System.Threading;
using System.Threading.Tasks;

using ConvocadoFc.Application.Handlers.Modules.Notifications.Models;
using ConvocadoFc.Domain.Models.Modules.Notifications;

namespace ConvocadoFc.Application.Handlers.Modules.Notifications.Interfaces;

public interface INotificationProvider
{
    NotificationChannel Channel { get; }
    Task SendAsync(NotificationRequest request, CancellationToken cancellationToken = default);
}
