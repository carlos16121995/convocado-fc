namespace ConvocadoFc.WebApi.Modules.Teams.Models;

/// <summary>
/// Solicitação para revisar uma solicitação de entrada.
/// </summary>
/// <param name="Approve">Indica se a solicitação deve ser aprovada.</param>
public sealed record ReviewJoinRequestRequest(
    bool Approve
);
