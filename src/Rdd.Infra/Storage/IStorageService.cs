using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rdd.Infra.Storage
{
    public interface IStorageService
    {
        IQueryable<TEntity> Set<TEntity>() where TEntity : class;
        Task<IEnumerable<TEntity>> EnumerateEntitiesAsync<TEntity>(IQueryable<TEntity> entities) where TEntity : class;
        void Add<TEntity>(TEntity entity) where TEntity : class;
        void AddRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class;
        void Remove<TEntity>(TEntity entity) where TEntity : class;
        void DiscardChanges<TEntity>(TEntity entity) where TEntity : class;
    }
}