using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rdd.Domain
{
    public interface IRestCollection<TEntity, TKey>
        where TEntity : class, IEntityBase<TKey>
        where TKey : IEquatable<TKey>
    {
        Task<TEntity> CreateAsync(ICandidate<TEntity, TKey> candidate);
        Task<IEnumerable<TEntity>> CreateAsync(IEnumerable<ICandidate<TEntity, TKey>> candidates);

        Task<TEntity> UpdateByIdAsync(TKey id, ICandidate<TEntity, TKey> candidate);
        Task<IEnumerable<TEntity>> UpdateByIdsAsync(IDictionary<TKey, ICandidate<TEntity, TKey>> candidatesByIds);

        Task DeleteByIdAsync(TKey id);
        Task DeleteByIdsAsync(IEnumerable<TKey> ids);
    }
}
