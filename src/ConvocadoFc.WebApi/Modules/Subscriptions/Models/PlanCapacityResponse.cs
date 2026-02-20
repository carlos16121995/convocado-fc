namespace ConvocadoFc.WebApi.Modules.Subscriptions.Models;

public sealed record PlanCapacityResponse(
    int? MaxTeams,
    int? MaxMembersPerTeam
);
