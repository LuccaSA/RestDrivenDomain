using RDD.Domain.Models.Querying;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RDD.Domain
{
    public interface IRepository<TEntity>
        where TEntity : class, IEntityBase
    {
        Task<int> CountAsync(Query<TEntity> query = null);
        Task<IEnumerable<TEntity>> EnumerateAsync(Query<TEntity> query = null);
        Task<IEnumerable<TEntity>> PrepareAsync(IEnumerable<TEntity> entities, Query<TEntity> query = null);
        void Add(TEntity entity);
        void AddRange(IEnumerable<TEntity> entities);
        void Remove(TEntity entity);
    }
}
