using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RDD.Domain.Models.Querying;

namespace RDD.Domain
{
    public interface IReadOnlyRestCollection<TEntity, TKey> 
        where TEntity : class, IEntityBase<TKey>
        where TKey : IEquatable<TKey>
    {
        Task<IReadOnlyCollection<TEntity>> GetAsync(Query<TEntity> query);
        Task<IReadOnlyCollection<TEntity>> GetAllAsync();
        Task<bool> AnyAsync(Query<TEntity> query);
        Task<TEntity> GetByIdAsync(TKey id, Query<TEntity> query);
    }
}
