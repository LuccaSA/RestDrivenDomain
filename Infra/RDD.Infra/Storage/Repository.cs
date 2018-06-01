using RDD.Domain;
using System.Collections.Generic;

namespace RDD.Infra.Storage
{
    public class Repository<TEntity> : ReadOnlyRepository<TEntity>, IRepository<TEntity>
        where TEntity : class, IEntityBase
    {
        public Repository(IStorageService storageService, IExecutionContext executionContext, ICombinationsHolder combinationsHolder)
            : base(storageService, executionContext, combinationsHolder) { }

        public virtual void Add(TEntity entity)
        {
            StorageService.Add(entity);
        }

        public virtual void AddRange(IEnumerable<TEntity> entities)
        {
            StorageService.AddRange(entities);
        }

        public virtual void Remove(TEntity entity)
        {
            StorageService.Remove(entity);
        }
    }
}
