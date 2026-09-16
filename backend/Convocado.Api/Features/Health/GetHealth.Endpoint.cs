namespace Convocado.Api.Features.Health;

public static partial class GetHealth
{
    public static IEndpointRouteBuilder MapGetHealth(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/health", (Handler handler) => handler.Handle(new Query()))
            .WithName("GetHealth")
            .WithTags("Health");

        return endpoints;
    }
}
