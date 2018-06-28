using RDD.Domain.Models.Querying;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RDD.Domain
{
    public interface IReadOnlyRepository<TEntity>
        where TEntity : class
    {
        Task<int> CountAsync(Query<TEntity> query);
        Task<IEnumerable<TEntity>> GetAsync(Query<TEntity> query);
        Task<IEnumerable<TEntity>> PrepareAsync(IEnumerable<TEntity> entities, Query<TEntity> query);
    }
}
