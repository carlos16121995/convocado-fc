namespace ConvocadoFc.Infrastructure.Notifications.Email;

public sealed class EmailSettings
{
    public string FromEmail { get; init; } = string.Empty;
    public string? FromName { get; init; }

    public string SmtpHost { get; init; } = string.Empty;
    public int SmtpPort { get; init; } = 587;
    public string? SmtpUser { get; init; }
    public string? SmtpPassword { get; init; }
    public bool EnableSsl { get; init; } = true;
    public bool UseDefaultCredentials { get; init; } = false;

    public string TemplatesPath { get; init; } = "Notifications/Email/Templates";
    public string BaseTemplateFile { get; init; } = "template_base.html";
    public string ContentTemplateFile { get; init; } = "template_base_conteudo.html";
}
