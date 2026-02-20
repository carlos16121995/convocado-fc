using System.Collections.Generic;

namespace ConvocadoFc.Application.Abstractions.Notifications.Models;

public sealed record EmailMessage(
    string Subject,
    string HtmlBody,
    IReadOnlyCollection<string> To,
    IReadOnlyCollection<string>? Cc = null
);
