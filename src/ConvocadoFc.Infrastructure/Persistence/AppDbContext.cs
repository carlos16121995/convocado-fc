using ConvocadoFc.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace ConvocadoFc.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IApplicationDbContext
{
    public IQueryable<TEntity> Query<TEntity>() where TEntity : class
        => Set<TEntity>().AsNoTracking();
}
