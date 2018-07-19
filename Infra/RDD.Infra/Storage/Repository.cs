using RDD.Domain;
using RDD.Domain.Rights;
using System.Collections.Generic;

namespace RDD.Infra.Storage
{
    public class Repository<TEntity> : ReadOnlyRepository<TEntity>, IRepository<TEntity>
        where TEntity : class
    {
        public Repository(IStorageService storageService, IRightsService rightsService)
            : base(storageService, rightsService) { }

        public virtual void Add(TEntity entity)
        {
            _storageService.Add(entity);
        }

        public virtual void AddRange(IEnumerable<TEntity> entities)
        {
            _storageService.AddRange(entities);
        }

        public virtual void Remove(TEntity entity)
        {
            _storageService.Remove(entity);
        }
    }
}
