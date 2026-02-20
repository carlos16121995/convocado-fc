namespace ConvocadoFc.WebApi.Modules.Subscriptions.Models;

public sealed record CreatePlanRequest(
    string Name,
    string Code,
    decimal? Price,
    string Currency,
    int? MaxTeams,
    int? MaxMembersPerTeam,
    bool IsCustomPricing,
    bool IsActive
);
