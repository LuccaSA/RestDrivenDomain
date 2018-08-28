using RDD.Domain.Models.Querying;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RDD.Domain
{
    public interface IRestCollection<TEntity, TKey> : IReadOnlyRestCollection<TEntity, TKey>
        where TEntity : class, IEntityBase<TKey>
        where TKey : IEquatable<TKey>
    {
        TEntity Create(ICandidate<TEntity, TKey> candidate, Query<TEntity> query = null);
        IEnumerable<TEntity> Create(IEnumerable<ICandidate<TEntity, TKey>> candidates, Query<TEntity> query = null);

        Task<TEntity> UpdateByIdAsync(TKey id, ICandidate<TEntity, TKey> candidate, Query<TEntity> query = null);
        Task<IEnumerable<TEntity>> UpdateByIdsAsync(IDictionary<TKey, ICandidate<TEntity, TKey>> candidatesByIds, Query<TEntity> query = null);

        Task<TEntity> DeleteByIdAsync(TKey id);
        Task<IEnumerable<TEntity>> DeleteByIdsAsync(IList<TKey> ids);
    }
}
