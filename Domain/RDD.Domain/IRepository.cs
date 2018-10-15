using System.Collections.Generic;

namespace Rdd.Domain
{
    public interface IRepository<TEntity> : IReadOnlyRepository<TEntity>
        where TEntity : class
    {
        void Add(TEntity entity);
        void AddRange(IEnumerable<TEntity> entities);
        void Remove(TEntity entity);
        void DiscardChanges(TEntity entity);
    }
}
