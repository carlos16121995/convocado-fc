using ConvocadoFc.Domain.Models.Modules.Teams;

namespace ConvocadoFc.WebApi.Modules.Teams.Models;

/// <summary>
/// Filtros para listagem de solicitações de entrada.
/// </summary>
public sealed record ListJoinRequestsQueryModel
{
    /// <summary>
    /// Filtra pelo status da solicitação.
    /// </summary>
    public ETeamJoinRequestStatus? Status { get; init; }

    /// <summary>
    /// Ordenação dos resultados.
    /// </summary>
    public string? OrderBy { get; init; }

    /// <summary>
    /// Página atual.
    /// </summary>
    public int Page { get; init; } = 1;

    /// <summary>
    /// Tamanho da página.
    /// </summary>
    public int PageSize { get; init; } = 20;
}
