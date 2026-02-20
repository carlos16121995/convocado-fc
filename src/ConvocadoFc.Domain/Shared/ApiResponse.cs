namespace ConvocadoFc.Domain.Shared;

public record ApiResponse
{
    public int StatusCode { get; init; }
    public string Message { get; init; } = string.Empty;
    public bool Success { get; init; }
    public List<ValidationFailure> Errors { get; init; } = new();
}

public record ApiResponse<T>
{
    public int StatusCode { get; init; }
    public string Message { get; init; } = string.Empty;
    public bool Success { get; init; }
    public List<ValidationFailure> Errors { get; init; } = new();
    public T? Data { get; init; }
}
