using RDD.Domain.Models.Querying;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RDD.Domain
{
    public interface IReadOnlyRestCollection<TEntity>
        where TEntity : class, IEntityBase
    {
        Task<ISelection<TEntity>> GetAsync(Query<TEntity> query);
        Task<IEnumerable<TEntity>> GetAllAsync();

        Task<bool> AnyAsync(Query<TEntity> query);
    }

    public interface IReadOnlyRestCollection<TEntity, TKey> : IReadOnlyRestCollection<TEntity>
        where TEntity : class, IEntityBase<TKey>
        where TKey : IEquatable<TKey>
    {
        Task<TEntity> GetByIdAsync(TKey id, Query<TEntity> query);
        Task<IEnumerable<TEntity>> GetByIdsAsync(IList<TKey> ids, Query<TEntity> query);
    }
}
