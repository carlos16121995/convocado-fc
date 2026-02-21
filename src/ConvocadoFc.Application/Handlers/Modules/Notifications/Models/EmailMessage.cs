namespace ConvocadoFc.Application.Handlers.Modules.Notifications.Models;

public sealed record EmailMessage(
    string Subject,
    string HtmlBody,
    IReadOnlyCollection<string> To,
    IReadOnlyCollection<string>? Cc = null
);
