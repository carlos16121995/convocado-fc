namespace ConvocadoFc.Application.Handlers.Modules.Subscriptions.Models;

public sealed record CreatePlanCommand(
    string Name,
    string Code,
    decimal? Price,
    string Currency,
    int? MaxTeams,
    int? MaxMembersPerTeam,
    bool IsCustomPricing,
    bool IsActive
);
