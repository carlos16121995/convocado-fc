using ConvocadoFc.Application.Abstractions.Notifications.Models;
using ConvocadoFc.Domain.Notifications;

namespace ConvocadoFc.Application.Abstractions.Notifications.Interfaces;

public interface INotificationProvider
{
    NotificationChannel Channel { get; }
    Task SendAsync(NotificationRequest request, CancellationToken cancellationToken = default);
}
