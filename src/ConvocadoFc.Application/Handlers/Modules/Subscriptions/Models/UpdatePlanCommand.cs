namespace ConvocadoFc.Application.Handlers.Modules.Subscriptions.Models;

public sealed record UpdatePlanCommand(
    Guid PlanId,
    string Name,
    string Code,
    decimal? Price,
    string Currency,
    int? MaxTeams,
    int? MaxMembersPerTeam,
    bool IsCustomPricing,
    bool IsActive
);
