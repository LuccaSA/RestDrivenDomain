using System;
using System.Collections.Generic;

namespace RDD.Domain
{
    public interface IRepository<TEntity> : IReadOnlyRepository<TEntity>
        where TEntity : class
    {
        void Add(TEntity entity);
        void AddRange(IEnumerable<TEntity> entities);
        void Remove(TEntity entity);

        void Update<T,TKey>(TKey id, TEntity entity)
            where T : class, TEntity, IEntityBase<T, TKey>
            where TKey : IEquatable<TKey>;
    }
}
