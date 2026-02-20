using System.Net;
using System.Net.Mail;

using ConvocadoFc.Application.Abstractions;
using ConvocadoFc.Application.Handlers.Modules.Authentication.Interfaces;
using ConvocadoFc.Application.Handlers.Modules.Notifications.Interfaces;
using ConvocadoFc.Application.Handlers.Modules.Notifications.Models;
using ConvocadoFc.Domain.Models.Modules.Users.Identity;
using ConvocadoFc.Infrastructure.Modules.Authentication;
using ConvocadoFc.Infrastructure.Modules.Notifications;
using ConvocadoFc.Infrastructure.Modules.Notifications.Email;
using ConvocadoFc.Infrastructure.Persistence;

using FluentEmail.Smtp;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using StackExchange.Redis;
using System;

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

        services
            .AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IPasswordValidator<ApplicationUser>, RegexPasswordValidator>();

        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.Configure<RefreshTokenOptions>(configuration.GetSection("RefreshToken"));
        services.Configure<AuthCookieOptions>(configuration.GetSection("AuthCookies"));

        var redisConnection = configuration.GetSection("Redis:ConnectionString").Value;
        if (string.IsNullOrWhiteSpace(redisConnection))
        {
            throw new InvalidOperationException("Redis connection string is required for refresh token storage.");
        }

        services.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(redisConnection));

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IRefreshTokenManager, RefreshTokenManager>();

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
