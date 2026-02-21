using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ConvocadoFc.Application.Handlers.Modules.Users.Interfaces;
using ConvocadoFc.Application.Handlers.Modules.Users.Implementations;
using ConvocadoFc.Application.Handlers.Modules.Subscriptions.Implementations;
using ConvocadoFc.Application.Handlers.Modules.Subscriptions.Interfaces;
using ConvocadoFc.Application.Handlers.Modules.Teams.Implementations;
using ConvocadoFc.Application.Handlers.Modules.Teams.Interfaces;
using ConvocadoFc.Application.Handlers.Modules.Authentication.Implementations;
using ConvocadoFc.Application.Handlers.Modules.Authentication.Interfaces;

namespace ConvocadoFc.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddScoped<IRegisterUserHandler, RegisterUserHandler>();
        services.AddScoped<IAuthHandler, AuthHandler>();
        services.AddScoped<IUserRolesHandler, UserRolesHandler>();
        services.AddScoped<IPlanManagementHandler, PlanManagementHandler>();
        services.AddScoped<ISubscriptionManagementHandler, SubscriptionManagementHandler>();
        services.AddScoped<ISubscriptionAccessService, SubscriptionAccessService>();
        services.AddScoped<ITeamManagementHandler, TeamManagementHandler>();
        services.AddScoped<ITeamInvitationHandler, TeamInvitationHandler>();
        services.AddScoped<ITeamAuthorizationHandler, TeamAuthorizationHandler>();
        services.AddScoped<ITeamSettingsHandler, TeamSettingsHandler>();
        services.AddScoped<ITeamPlayerHandler, TeamPlayerHandler>();

        return services;
    }
}
