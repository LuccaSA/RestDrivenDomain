using System;
using System.Threading;
using System.Threading.Tasks;
using Rdd.Domain.Models.Querying;

namespace Rdd.Domain
{
    public interface IReadOnlyRestCollection<TEntity, TKey> 
        where TEntity : class, IEntityBase<TKey>
        where TKey : IEquatable<TKey>
    {
        Task<ISelection<TEntity>> GetAsync(Query<TEntity> query, CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(Query<TEntity> query, CancellationToken cancellationToken = default);
        Task<TEntity> GetByIdAsync(TKey id, Query<TEntity> query, CancellationToken cancellationToken = default);
    }
}
