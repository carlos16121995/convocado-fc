namespace Convocado.Api.Infrastructure.Http;

public record ApiResponse(
    Guid CorrelationId,
    ApiError? Error = null,
    IReadOnlyDictionary<string, string[]>? Errors = null)
{
    public bool Succeeded => Error is null;

    public static ApiResponse Failure(
        ApiError error,
        Guid correlationId,
        IReadOnlyDictionary<string, string[]>? errors = null) =>
        new(correlationId, error, errors);
}

public sealed record ApiResponse<T>(
    T? Data,
    Guid CorrelationId,
    PageInfo? Pagination = null,
    ApiError? Error = null,
    IReadOnlyDictionary<string, string[]>? Errors = null)
    : ApiResponse(CorrelationId, Error, Errors)
{
    public static ApiResponse<T> Success(T data, Guid correlationId, PageInfo? pagination = null) =>
        new(data, correlationId, pagination);

    public new static ApiResponse<T> Failure(
        ApiError error,
        Guid correlationId,
        IReadOnlyDictionary<string, string[]>? errors = null) =>
        new(default, correlationId, null, error, errors);
}

public sealed record ApiError(string Code, string Message);

public sealed record PageInfo(int Page, int PageSize, long TotalItems, int TotalPages);
