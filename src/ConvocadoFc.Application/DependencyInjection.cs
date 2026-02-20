using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ConvocadoFc.Application.Handlers.Modules.Users.Interfaces;
using ConvocadoFc.Application.Handlers.Modules.Users.Implementations;
using ConvocadoFc.Application.Handlers.Modules.Subscriptions.Implementations;
using ConvocadoFc.Application.Handlers.Modules.Subscriptions.Interfaces;

namespace ConvocadoFc.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddScoped<IRegisterUserHandler, RegisterUserHandler>();
        services.AddScoped<IPlanManagementHandler, PlanManagementHandler>();
        services.AddScoped<ISubscriptionManagementHandler, SubscriptionManagementHandler>();
        services.AddScoped<ISubscriptionAccessService, SubscriptionAccessService>();

        return services;
    }
}
