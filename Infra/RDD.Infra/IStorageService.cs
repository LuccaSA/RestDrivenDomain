using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RDD.Infra
{
    public interface IStorageService<TEntity> where TEntity : class
    {
        IQueryable<TEntity> Set();
        Task<IEnumerable<TEntity>> EnumerateEntitiesAsync(IQueryable<TEntity> entities);
        void Add(TEntity entity);
        void AddRange(IEnumerable<TEntity> entities);
        void Remove(TEntity entity);
        Task SaveChangesAsync();
        void AddAfterSaveChangesAction(Task action);
    }
}