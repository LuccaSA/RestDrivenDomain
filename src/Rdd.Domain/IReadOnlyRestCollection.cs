﻿using System;
using System.Threading.Tasks;
using Rdd.Domain.Models.Querying;

namespace Rdd.Domain
{
    public interface IReadOnlyRestCollection<TEntity, TKey> 
        where TEntity : class, IEntityBase<TKey>
        where TKey : IEquatable<TKey>
    {
        Task<ISelection<TEntity>> GetAsync(IQuery<TEntity> query);
        Task<TEntity> GetByIdAsync(TKey id, IQuery<TEntity> query);
    }
}
