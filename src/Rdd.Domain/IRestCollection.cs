﻿using Rdd.Domain.Models.Querying;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rdd.Domain
{
    public interface IRestCollection<TEntity, TKey> : IReadOnlyRestCollection<TEntity, TKey>
        where TEntity : class, IEntityBase<TKey>
        where TKey : IEquatable<TKey>
    {
        Task<TEntity> CreateAsync(ICandidate<TEntity, TKey> candidate, Query<TEntity> query);
        Task<IEnumerable<TEntity>> CreateAsync(IEnumerable<ICandidate<TEntity, TKey>> candidates, Query<TEntity> query);
        Task<IEnumerable<TEntity>> CreateAsync(IEnumerable<TEntity> entities, bool checkRights = true);

        Task<TEntity> UpdateByIdAsync(TKey id, ICandidate<TEntity, TKey> candidate, Query<TEntity> query = null);
        Task<IEnumerable<TEntity>> UpdateByIdsAsync(IDictionary<TKey, ICandidate<TEntity, TKey>> candidatesByIds, Query<TEntity> query = null);

        Task DeleteByIdAsync(TKey id);
        Task DeleteByIdsAsync(IEnumerable<TKey> ids);
    }
}
