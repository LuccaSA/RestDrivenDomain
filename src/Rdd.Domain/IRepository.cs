using Rdd.Domain.Models.Querying;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rdd.Domain
{
    public interface IRepository<TEntity> : IReadOnlyRepository<TEntity>
        where TEntity : class
    {
        Task AddAsync(TEntity entity, Query<TEntity> query);
        Task AddRangeAsync(IEnumerable<TEntity> entities, Query<TEntity> query);

        void Remove(TEntity entity);
        void DiscardChanges(TEntity entity);
    }
}
