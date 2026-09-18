using Convocado.Api.Infrastructure.Http;

namespace Convocado.Api.Features.Health;

public static partial class GetHealth
{
    public static IEndpointRouteBuilder MapGetHealth(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/health", async (HttpContext context, Handler handler, CancellationToken cancellationToken) =>
            {
                var response = await handler.Handle(new Query(), cancellationToken);
                var correlationId = CorrelationIdMiddleware.GetCorrelationId(context);

                return response.Status == "Healthy"
                    ? Results.Ok(ApiResponse<Response>.Success(response, correlationId))
                    : Results.Json(
                        ApiResponse<Response>.Failure(
                            new ApiError("database_unavailable", "A base de dados está indisponível."),
                            correlationId),
                        statusCode: StatusCodes.Status503ServiceUnavailable);
            })
            .WithName("GetHealth")
            .WithTags("Health")
            .Produces<ApiResponse<Response>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<Response>>(StatusCodes.Status503ServiceUnavailable);

        return endpoints;
    }
}
