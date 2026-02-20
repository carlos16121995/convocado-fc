using ConvocadoFc.Domain.Notifications;

namespace ConvocadoFc.Application.Abstractions.Notifications.Models;

public sealed record NotificationRequest(
    NotificationChannel Channel,
    string Reason,
    string Title,
    string Message,
    string ActionUrl,
    IReadOnlyCollection<string> To,
    IReadOnlyCollection<string>? Cc = null,
    Guid TriggeredByUserId = default,
    Guid TeamId = default
);
