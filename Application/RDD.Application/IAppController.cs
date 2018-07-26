﻿using RDD.Domain;
using RDD.Domain.Models.Querying;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RDD.Application
{
    public interface IAppController<TEntity, TKey> : IReadOnlyAppController<TEntity, TKey>
        where TEntity : class, IEntityBase<TEntity, TKey>
        where TKey : IEquatable<TKey>
    {
        Task<TEntity> CreateAsync(ICandidate<TEntity, TKey> candidate, Query<TEntity> query);
        Task<IReadOnlyCollection<TEntity>> CreateAsync(IEnumerable<ICandidate<TEntity, TKey>> candidates, Query<TEntity> query);

        Task<TEntity> UpdateByIdAsync(TKey id, ICandidate<TEntity, TKey> candidate, Query<TEntity> query);
        Task<IReadOnlyCollection<TEntity>> UpdateByIdsAsync(IDictionary<TKey, ICandidate<TEntity, TKey>> candidatesByIds, Query<TEntity> query);
        Task DeleteByIdAsync(TKey id);
        Task DeleteByIdsAsync(IEnumerable<TKey> ids);
    }
}
