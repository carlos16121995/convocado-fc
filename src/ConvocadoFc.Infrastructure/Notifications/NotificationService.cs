using ConvocadoFc.Application.Abstractions;
using ConvocadoFc.Application.Abstractions.Notifications.Interfaces;
using ConvocadoFc.Application.Abstractions.Notifications.Models;
using ConvocadoFc.Domain.Notifications;

namespace ConvocadoFc.Infrastructure.Notifications;

public sealed class NotificationService(IEnumerable<INotificationProvider> providers, IApplicationDbContext dbContext) : INotificationService
{
    private readonly IReadOnlyDictionary<NotificationChannel, INotificationProvider> _providers = providers.ToDictionary(static provider => provider.Channel);

    public async Task SendAsync(NotificationRequest request, CancellationToken cancellationToken = default)
    {
        if (!_providers.TryGetValue(request.Channel, out var provider))
        {
            throw new NotSupportedException($"Canal de notificação não suportado: {request.Channel}");
        }

        var log = new NotificationLog
        {
            SentAt = DateTimeOffset.UtcNow,
            Reason = request.Reason,
            Channel = request.Channel,
            TriggeredByUserId = request.TriggeredByUserId,
            TeamId = request.TeamId,
            Title = request.Title,
            Message = request.Message,
            ActionUrl = request.ActionUrl,
        };

        try
        {
            await provider.SendAsync(request, cancellationToken);
            log.IsSuccess = true;
        }
        catch (Exception ex)
        {
            log.IsSuccess = false;
            log.ErrorMessage = ex.Message;
            throw;
        }
        finally
        {
            await dbContext.AddAsync(log, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
