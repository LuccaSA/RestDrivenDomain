using System;
using System.Collections.Generic;

namespace Rdd.Domain
{
    public interface IRepository<TEntity, TKey> : IReadOnlyRepository<TEntity, TKey>
        where TEntity : class, IPrimaryKey<TKey>
        where TKey : IEquatable<TKey>
    {
        void Add(TEntity entity);
        void AddRange(IEnumerable<TEntity> entities);
        void Remove(TEntity entity);
        void DiscardChanges(TEntity entity);
    }
}
