using System.Net;
using System.Net.Mail;

using ConvocadoFc.Application.Abstractions;
using ConvocadoFc.Application.Abstractions.Notifications.Interfaces;
using ConvocadoFc.Application.Abstractions.Notifications.Models;
using ConvocadoFc.Infrastructure.Notifications;
using ConvocadoFc.Infrastructure.Notifications.Email;
using ConvocadoFc.Infrastructure.Persistence;

using FluentEmail.Smtp;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ConvocadoFc.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(
                connectionString,
                npgsql => npgsql.UseNetTopologySuite()));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        var emailSection = configuration.GetSection("Email");
        services.Configure<EmailSettings>(emailSection);

        var emailSettings = emailSection.Get<EmailSettings>() ?? new EmailSettings();

        services
            .AddFluentEmail(emailSettings.FromEmail, emailSettings.FromName)
            .AddSmtpSender(() =>
            {
                var client = new SmtpClient(emailSettings.SmtpHost, emailSettings.SmtpPort)
                {
                    EnableSsl = emailSettings.EnableSsl
                };

                if (emailSettings.UseDefaultCredentials)
                {
                    client.UseDefaultCredentials = true;
                }
                else if (!string.IsNullOrWhiteSpace(emailSettings.SmtpUser))
                {
                    client.Credentials = new NetworkCredential(emailSettings.SmtpUser, emailSettings.SmtpPassword);
                }

                return client;
            });

        services.AddSingleton<IEmailTemplateRenderer, EmailTemplateRenderer>();
        services.AddScoped<IMessageTransport<EmailMessage>, FluentEmailTransport>();
        services.AddScoped<INotificationProvider, EmailNotificationProvider>();
        services.AddScoped<INotificationService, NotificationService>();

        return services;
    }
}
