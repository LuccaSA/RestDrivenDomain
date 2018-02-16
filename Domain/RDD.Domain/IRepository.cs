using RDD.Domain.Models.Querying;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RDD.Domain
{
    public interface IRepository<TEntity>
        where TEntity : class, IEntityBase
    {
        Task<int> CountAsync();
        Task<int> CountAsync(Query<TEntity> query);
        Task<IEnumerable<TEntity>> EnumerateAsync();
        Task<IEnumerable<TEntity>> EnumerateAsync(Query<TEntity> query);
        Task<IEnumerable<TEntity>> PrepareAsync(IEnumerable<TEntity> entities);
        Task<IEnumerable<TEntity>> PrepareAsync(IEnumerable<TEntity> entities, Query<TEntity> query);
        void Add(TEntity entity);
        void AddRange(IEnumerable<TEntity> entities);
        void Remove(TEntity entity);
    }
}
