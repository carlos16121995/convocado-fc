namespace ConvocadoFc.WebApi.Modules.Teams.Models;

/// <summary>
/// Solicitação para atualização de um time.
/// </summary>
/// <param name="TeamId">Identificador do time.</param>
/// <param name="Name">Nome do time.</param>
/// <param name="HomeFieldName">Nome do campo principal.</param>
/// <param name="HomeFieldAddress">Endereço do campo principal.</param>
/// <param name="HomeFieldLatitude">Latitude do campo.</param>
/// <param name="HomeFieldLongitude">Longitude do campo.</param>
/// <param name="CrestUrl">URL do brasão do time.</param>
/// <param name="IsActive">Indica se o time está ativo.</param>
public sealed record UpdateTeamRequest(
    Guid TeamId,
    string Name,
    string HomeFieldName,
    string? HomeFieldAddress,
    decimal? HomeFieldLatitude,
    decimal? HomeFieldLongitude,
    string? CrestUrl,
    bool IsActive
);
