using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConvocadoFc.Application.Abstractions;

public interface IApplicationDbContext
{
    IQueryable<TEntity> Query<TEntity>() where TEntity : class;
    IQueryable<TEntity> Track<TEntity>() where TEntity : class;
    Task AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class;
    Task UpdateAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class;
    Task RemoveAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class;
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
