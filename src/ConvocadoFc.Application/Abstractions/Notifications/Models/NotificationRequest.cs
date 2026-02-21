using ConvocadoFc.Domain.Notifications;

using System;
using System.Collections.Generic;

namespace ConvocadoFc.Application.Abstractions.Notifications.Models;

public sealed record NotificationRequest(
    ENotificationChannel Channel,
    string Reason,
    string Title,
    string Message,
    string ActionUrl,
    IReadOnlyCollection<string> To,
    IReadOnlyCollection<string>? Cc = null,
    Guid TriggeredByUserId = default,
    Guid TeamId = default
);
