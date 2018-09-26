using RDD.Domain.Models.Querying;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RDD.Domain
{
    public interface IReadOnlyRepository<TEntity>
        where TEntity : class
    {
        Task<int> CountAsync(Query<TEntity> query);
        Task<IReadOnlyCollection<TEntity>> GetAsync(Query<TEntity> query);
        Task<IReadOnlyCollection<TEntity>> PrepareAsync(IEnumerable<TEntity> entities, Query<TEntity> query);
    }
}
