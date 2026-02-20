namespace ConvocadoFc.Application.Handlers.Modules.Subscriptions.Models;

public sealed record PlanCapacityDto(
    int? MaxTeams,
    int? MaxMembersPerTeam
);
