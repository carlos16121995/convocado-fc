namespace ConvocadoFc.WebApi.Modules.Teams.Models;

/// <summary>
/// Dados resumidos de um time.
/// </summary>
/// <param name="Id">Identificador do time.</param>
/// <param name="OwnerUserId">Identificador do proprietário.</param>
/// <param name="Name">Nome do time.</param>
/// <param name="HomeFieldName">Nome do campo principal.</param>
/// <param name="HomeFieldAddress">Endereço do campo principal.</param>
/// <param name="HomeFieldLatitude">Latitude do campo.</param>
/// <param name="HomeFieldLongitude">Longitude do campo.</param>
/// <param name="CrestUrl">URL do brasão do time.</param>
/// <param name="IsActive">Indica se o time está ativo.</param>
/// <param name="CreatedAt">Data de criação.</param>
/// <param name="UpdatedAt">Data da última atualização.</param>
public sealed record TeamResponse(
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
