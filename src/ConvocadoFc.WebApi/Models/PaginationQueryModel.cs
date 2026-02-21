namespace ConvocadoFc.WebApi.Models;

/// <summary>
/// Parâmetros de paginação para consultas.
/// </summary>
public record PaginationQueryModel
{
    /// <summary>
    /// Campo de ordenação dos resultados.
    /// </summary>
    public string? OrderBy { get; init; }

    /// <summary>
    /// Tamanho da página.
    /// </summary>
    public int PageSize { get; init; } = 20;

    /// <summary>
    /// Página atual.
    /// </summary>
    public int Page { get; init; } = 1;
}
