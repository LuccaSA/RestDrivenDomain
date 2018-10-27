using Rdd.Domain;
using Rdd.Domain.Models.Querying;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rdd.Application
{
    public interface IAppController<TEntity, TKey> : IReadOnlyAppController<TEntity, TKey>
        where TEntity : class, IEntityBase<TKey>
        where TKey : IEquatable<TKey>
    {
        Task<TEntity> CreateAsync(ICandidate<TEntity, TKey> candidate, IQuery<TEntity> query);
        Task<IEnumerable<TEntity>> CreateAsync(IEnumerable<ICandidate<TEntity, TKey>> candidates, IQuery<TEntity> query);

        Task<TEntity> UpdateByIdAsync(TKey id, ICandidate<TEntity, TKey> candidate, IQuery<TEntity> query);
        Task<IEnumerable<TEntity>> UpdateByIdsAsync(IDictionary<TKey, ICandidate<TEntity, TKey>> candidatesByIds, IQuery<TEntity> query);

        Task DeleteByIdAsync(TKey id, IQuery<TEntity> query);
        Task DeleteByIdsAsync(IEnumerable<TKey> ids, IQuery<TEntity> query);
    }
}
