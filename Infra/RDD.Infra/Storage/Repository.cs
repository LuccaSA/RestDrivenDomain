using Rdd.Application;
using Rdd.Domain;
using Rdd.Domain.Rights;
using System.Collections.Generic;

namespace Rdd.Infra.Storage
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

        public virtual void Remove(TEntity entity)
        {
            StorageService.Remove(entity);
        }
    }
}
