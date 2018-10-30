using Rdd.Domain.Models.Querying;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rdd.Domain
{
    public interface IReadOnlyRepository<TEntity, TKey>
        where TEntity : class, IPrimaryKey<TKey>
        where TKey : IEquatable<TKey>
    {
        Task<int> CountAsync(IQuery<TEntity> query);
        Task<ISelection<TEntity>> GetAsync(IQuery<TEntity> query);
        Task<TEntity> GetByIdAsync(TKey id, IQuery<TEntity> query);
        Task<IEnumerable<TEntity>> GetByIdsAsync(IEnumerable<TKey> ids, IQuery<TEntity> query);
    }
}
