using ConvocadoFc.Application.Abstractions;
using ConvocadoFc.Domain.Notifications;
using Microsoft.EntityFrameworkCore;

namespace ConvocadoFc.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IApplicationDbContext
{
    public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();

    public IQueryable<TEntity> Query<TEntity>() where TEntity : class
        => Set<TEntity>().AsNoTracking();

    async Task IApplicationDbContext.AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken)
        => await Set<TEntity>().AddAsync(entity, cancellationToken);
}
