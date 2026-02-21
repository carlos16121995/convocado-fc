namespace ConvocadoFc.WebApi.Modules.Teams.Models;

/// <summary>
/// Filtros para listagem de jogadores do time.
/// </summary>
public sealed record ListPlayersQueryModel
{
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
