namespace ConvocadoFc.WebApi.Modules.Teams.Models;

/// <summary>
/// Solicitação para criação de um time.
/// </summary>
/// <param name="Name">Nome do time.</param>
/// <param name="HomeFieldName">Nome do campo principal.</param>
/// <param name="HomeFieldAddress">Endereço do campo principal.</param>
/// <param name="HomeFieldLatitude">Latitude do campo.</param>
/// <param name="HomeFieldLongitude">Longitude do campo.</param>
/// <param name="CrestUrl">URL do brasão do time.</param>
public sealed record CreateTeamRequest(
    string Name,
    string HomeFieldName,
    string? HomeFieldAddress,
    decimal? HomeFieldLatitude,
    decimal? HomeFieldLongitude,
    string? CrestUrl
);
