using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConvocadoFc.Application.Abstractions;

public interface IApplicationDbContext
{
    IQueryable<TEntity> Query<TEntity>() where TEntity : class;
    Task AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
