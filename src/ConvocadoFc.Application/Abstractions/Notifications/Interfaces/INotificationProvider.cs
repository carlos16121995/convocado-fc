using ConvocadoFc.Application.Abstractions.Notifications.Models;
using ConvocadoFc.Domain.Notifications;

using System.Threading;
using System.Threading.Tasks;

namespace ConvocadoFc.Application.Abstractions.Notifications.Interfaces;

public interface INotificationProvider
{
    ENotificationChannel Channel { get; }
    Task SendAsync(NotificationRequest request, CancellationToken cancellationToken = default);
}
