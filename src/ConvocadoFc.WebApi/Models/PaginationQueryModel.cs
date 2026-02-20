namespace ConvocadoFc.WebApi.Models;

public record PaginationQueryModel
{
    public string? OrderBy { get; init; }
    public int PageSize { get; init; } = 20;
    public int Page { get; init; } = 1;
}
