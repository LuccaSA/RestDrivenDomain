﻿using Rdd.Domain.Models.Querying;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rdd.Domain
{
    public interface IReadOnlyRepository<TEntity>
        where TEntity : class
    {
        Task<int> CountAsync(Query<TEntity> query);
        Task<IEnumerable<TEntity>> GetAsync(Query<TEntity> query);
        Task<IEnumerable<TEntity>> PrepareAsync(IEnumerable<TEntity> entities, Query<TEntity> query);
        Task<bool> AnyAsync(Query<TEntity> query);
    }
}
