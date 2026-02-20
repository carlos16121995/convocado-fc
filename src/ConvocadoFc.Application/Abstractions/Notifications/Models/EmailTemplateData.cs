namespace ConvocadoFc.Application.Abstractions.Notifications.Models;

public sealed record EmailTemplateData(
    string Title,
    string DynamicMessage,
    string ActionUrl
);
