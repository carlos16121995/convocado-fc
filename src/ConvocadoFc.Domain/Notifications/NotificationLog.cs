using System;

namespace ConvocadoFc.Domain.Notifications;

public sealed class NotificationLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset SentAt { get; set; }
    public string Reason { get; set; } = string.Empty;
    public NotificationChannel Channel { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }

    public Guid TriggeredByUserId { get; set; } = Guid.Empty; // TODO: Substituir pelo usuário real que disparou.
    public Guid TeamId { get; set; } = Guid.Empty; // TODO: Substituir pelo Time real.

    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string ActionUrl { get; set; } = string.Empty;
}
