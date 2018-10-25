using Rdd.Domain;
using Rdd.Infra.Rights;
using Rdd.Infra.Web.Models;
using System;
using System.Collections.Generic;

namespace Rdd.Infra.Storage
{
    public class Repository<TEntity, TKey> : ReadOnlyRepository<TEntity, TKey>, IRepository<TEntity, TKey>
        where TEntity : class, IPrimaryKey<TKey>
        where TKey : IEquatable<TKey>
    {
        public Repository(IStorageService storageService, IRightExpressionsHelper<TEntity> rightExpressionsHelper, HttpQuery<TEntity, TKey> httpQuery)
            : base(storageService, rightExpressionsHelper, httpQuery) { }

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

        public void DiscardChanges(TEntity entity)
        {
            StorageService.DiscardChanges(entity);
        }
    }
}
