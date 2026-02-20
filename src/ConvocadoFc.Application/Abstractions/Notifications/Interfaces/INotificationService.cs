using ConvocadoFc.Application.Abstractions.Notifications.Models;

namespace ConvocadoFc.Application.Abstractions.Notifications.Interfaces;

public interface INotificationService
{
    Task SendAsync(NotificationRequest request, CancellationToken cancellationToken = default);
}
