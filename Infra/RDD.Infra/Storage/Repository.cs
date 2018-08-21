using System;
using RDD.Domain;
using RDD.Domain.Rights;
using System.Collections.Generic;

namespace RDD.Infra.Storage
{
    public class Repository<TEntity> : ReadOnlyRepository<TEntity>, IRepository<TEntity>
        where TEntity : class
    {
        public Repository(IStorageService storageService, IRightExpressionsHelper<TEntity> rightExpressionsHelper)
            : base(storageService, rightExpressionsHelper) { }

        public virtual void Add(TEntity entity)
        {
            StorageService.Add(entity);
        }

        public virtual void AddRange(IEnumerable<TEntity> entities)
        {
            StorageService.AddRange(entities);
        }

        public virtual void Update<T,TKey>(TKey id, TEntity entity)
            where T : class, TEntity, IEntityBase<T, TKey>
            where TKey : IEquatable<TKey>
        {
            StorageService.Update(id, entity as T);
        }

        public virtual void Remove(TEntity entity)
        {
            StorageService.Remove(entity);
        }
    }
}
