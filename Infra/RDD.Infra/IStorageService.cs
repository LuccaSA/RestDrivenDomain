using RDD.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RDD.Infra
{
    public interface IStorageService : IDisposable
    {
        IQueryable<TEntity> Set<TEntity>() where TEntity : class;
        Task<IEnumerable<TEntity>> EnumerateEntitiesAsync<TEntity>(IQueryable<TEntity> entities) where TEntity : class;
        void Add<TEntity>(TEntity entity) where TEntity : class;
        void AddRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class;
        void Remove<TEntity>(TEntity entity) where TEntity : class;
        Task SaveChangesAsync();
        void AddAfterSaveChangesAction(Task action);

        void Update<TEntity, TKey>(TKey id, TEntity toUpdate)
            where TEntity : class, IEntityBase<TEntity, TKey>
            where TKey : IEquatable<TKey>;
    }
}
