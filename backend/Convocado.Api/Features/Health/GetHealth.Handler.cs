using Convocado.Api.Infrastructure.Persistence;

namespace Convocado.Api.Features.Health;

public static partial class GetHealth
{
    public sealed class Handler
    {
        private readonly QueryDbContext _queryDbContext;

        public Handler(QueryDbContext queryDbContext)
        {
            _queryDbContext = queryDbContext;
        }

        public async Task<Response> Handle(Query query, CancellationToken cancellationToken)
        {
            var databaseAvailable = await _queryDbContext.Database.CanConnectAsync(cancellationToken);

            return new Response(databaseAvailable ? "Healthy" : "Unhealthy");
        }
    }
}
