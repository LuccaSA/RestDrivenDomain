using Rdd.Domain.Models.Querying;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rdd.Domain
{
    public interface IRestCollection<TEntity, TKey> : IReadOnlyRestCollection<TEntity, TKey>
        where TEntity : class, IEntityBase<TKey>
        where TKey : IEquatable<TKey>
    {
        Task<TEntity> CreateAsync(ICandidate<TEntity, TKey> candidate, Query<TEntity> query = null);
        Task<IEnumerable<TEntity>> CreateAsync(IEnumerable<ICandidate<TEntity, TKey>> candidates, Query<TEntity> query = null);
        Task<IEnumerable<TEntity>> CreateAsync(IEnumerable<TEntity> entities);

        Task<TEntity> UpdateByIdAsync(TKey id, ICandidate<TEntity, TKey> candidate, Query<TEntity> query = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<TEntity>> UpdateByIdsAsync(IDictionary<TKey, ICandidate<TEntity, TKey>> candidatesByIds, Query<TEntity> query = null, CancellationToken cancellationToken = default);

        Task DeleteByIdAsync(TKey id, CancellationToken cancellationToken = default);
        Task DeleteByIdsAsync(IEnumerable<TKey> ids, CancellationToken cancellationToken = default);
    }
}
