using Microsoft.EntityFrameworkCore;

namespace Convocado.Api.Infrastructure.Persistence;

public abstract class ConvocadoDbContext(DbContextOptions options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension("postgis");
    }
}
