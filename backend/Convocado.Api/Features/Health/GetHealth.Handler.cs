namespace Convocado.Api.Features.Health;

public static partial class GetHealth
{
    public sealed class Handler
    {
        public Response Handle(Query query) => new("Healthy");
    }
}
