namespace ConvocadoFc.Domain.Shared;

public record PaginationQuery
{
    public string? OrderBy { get; init; }
    public int PageSize { get; init; } = 20;
    public int Page { get; init; } = 1;
}

public record PaginatedResult<T>
{
    public int TotalItems { get; init; }
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
}
