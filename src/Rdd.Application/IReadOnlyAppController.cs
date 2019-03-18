using Rdd.Domain;
using Rdd.Domain.Models.Querying;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rdd.Application
{
    public interface IReadOnlyAppController<TEntity, TKey>
        where TEntity : class, IEntityBase<TKey>
        where TKey : IEquatable<TKey>
    {
        Task<ISelection<TEntity>> GetAsync(Query<TEntity> query, CancellationToken cancellationToken = default);
        Task<TEntity> GetByIdAsync(TKey id, Query<TEntity> query, CancellationToken cancellationToken = default);
    }
}
