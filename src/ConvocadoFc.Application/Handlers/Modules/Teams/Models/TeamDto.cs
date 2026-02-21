namespace ConvocadoFc.Application.Handlers.Modules.Teams.Models;

public sealed record TeamDto(
    Guid Id,
    Guid OwnerUserId,
    string Name,
    string HomeFieldName,
    string? HomeFieldAddress,
    decimal? HomeFieldLatitude,
    decimal? HomeFieldLongitude,
    string? CrestUrl,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);
