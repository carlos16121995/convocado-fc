using System;
using System.Collections.Generic;

using ConvocadoFc.Domain.Models.Modules.Notifications;

namespace ConvocadoFc.Application.Handlers.Modules.Notifications.Models;

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
