using Rdd.Domain;
using Rdd.Domain.Helpers;
using Rdd.Domain.Rights;
using System.Collections.Generic;

namespace Rdd.Infra.Storage
{
    public class Repository<TEntity> : ReadOnlyRepository<TEntity>, IRepository<TEntity>
        where TEntity : class
    {
        public Repository(IStorageService storageService, IRightExpressionsHelper<TEntity> rightExpressionsHelper)
            : base(storageService, rightExpressionsHelper) { }

        public void Add(TEntity entity)
        {
            StorageService.AddRange(entity.Yield());
        }

        public virtual void AddRange(IEnumerable<TEntity> entities)
        {
            StorageService.AddRange(entities);
        }

        public virtual void Remove(TEntity entity)
        {
            StorageService.Remove(entity);
        }

        public void DiscardChanges(TEntity entity)
        {
            StorageService.DiscardChanges(entity);
        }
    }
}
