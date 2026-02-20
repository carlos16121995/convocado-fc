namespace ConvocadoFc.Application.Handlers.Modules.Notifications.Models;

public sealed record EmailTemplateData(
    string Title,
    string DynamicMessage,
    string ActionUrl
);
