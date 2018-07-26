using RDD.Domain;
using RDD.Domain.Models.Querying;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RDD.Application
{
    public interface IReadOnlyAppController<TEntity, TKey>
        where TEntity : class, IEntityBase<TKey>
        where TKey : IEquatable<TKey>
    {
        Task<IReadOnlyCollection<TEntity>> GetAsync(Query<TEntity> query);
        Task<TEntity> GetByIdAsync(TKey id, Query<TEntity> query);
    }
}
