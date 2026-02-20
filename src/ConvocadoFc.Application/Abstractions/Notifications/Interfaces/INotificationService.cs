using ConvocadoFc.Application.Abstractions.Notifications.Models;

using System.Threading;
using System.Threading.Tasks;

namespace ConvocadoFc.Application.Abstractions.Notifications.Interfaces;

public interface INotificationService
{
    Task SendAsync(NotificationRequest request, CancellationToken cancellationToken = default);
}
