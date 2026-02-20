namespace ConvocadoFc.Application.Abstractions;

public interface IApplicationDbContext
{
    IQueryable<TEntity> Query<TEntity>() where TEntity : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
