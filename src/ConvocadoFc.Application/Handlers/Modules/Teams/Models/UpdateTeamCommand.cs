namespace ConvocadoFc.Application.Handlers.Modules.Teams.Models;

public sealed record UpdateTeamCommand(
    Guid TeamId,
    Guid UpdatedByUserId,
    string Name,
    string HomeFieldName,
    string? HomeFieldAddress,
    decimal? HomeFieldLatitude,
    decimal? HomeFieldLongitude,
    string? CrestUrl,
    bool IsActive,
    bool IsSystemAdmin
);
