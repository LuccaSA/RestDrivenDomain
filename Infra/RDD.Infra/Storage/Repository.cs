using RDD.Domain;
using RDD.Domain.Rights;
using System.Collections.Generic;
using RDD.Domain.Models.Querying;

namespace RDD.Infra.Storage
{
    public class Repository<TEntity> : ReadOnlyRepository<TEntity>, IRepository<TEntity>
        where TEntity : class
    {
        public Repository(IStorageService storageService, IRightExpressionsHelper<TEntity> rightExpressionsHelper, QueryRequest queryRequest)
            : base(storageService, rightExpressionsHelper, queryRequest)
        {
        }

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
