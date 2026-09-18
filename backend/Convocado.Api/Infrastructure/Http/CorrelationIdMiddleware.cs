namespace Convocado.Api.Infrastructure.Http;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Correlation-Id";
    private const string ItemKey = "CorrelationId";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = Guid.TryParse(context.Request.Headers[HeaderName], out var requestCorrelationId)
            ? requestCorrelationId
            : Guid.NewGuid();

        context.Items[ItemKey] = correlationId;
        context.TraceIdentifier = correlationId.ToString();
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId.ToString();
            return Task.CompletedTask;
        });

        await next(context);
    }

    public static Guid GetCorrelationId(HttpContext context) =>
        context.Items.TryGetValue(ItemKey, out var value) && value is Guid correlationId
            ? correlationId
            : throw new InvalidOperationException("O correlation ID não foi inicializado.");
}
