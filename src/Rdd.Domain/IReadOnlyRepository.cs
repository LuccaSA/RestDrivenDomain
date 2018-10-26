using Rdd.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rdd.Domain
{
    public interface IReadOnlyRepository<TEntity, TKey>
        where TEntity : class, IPrimaryKey<TKey>
        where TKey : IEquatable<TKey>
    {
        Task<ISelection<TEntity>> GetAsync();
        Task<TEntity> GetAsync(TKey id);
        Task<IEnumerable<TEntity>> GetAsync(IEnumerable<TKey> ids);
    }
}
