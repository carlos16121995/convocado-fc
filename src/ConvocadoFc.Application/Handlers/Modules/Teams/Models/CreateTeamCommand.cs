namespace ConvocadoFc.Application.Handlers.Modules.Teams.Models;

public sealed record CreateTeamCommand(
    Guid OwnerUserId,
    string Name,
    string HomeFieldName,
    string? HomeFieldAddress,
    decimal? HomeFieldLatitude,
    decimal? HomeFieldLongitude,
    string? CrestUrl,
    bool IsSystemAdmin
);
