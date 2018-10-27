using Rdd.Domain.Models.Querying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rdd.Domain.Models
{
    public class ReadOnlyRestCollection<TEntity, TKey> : IReadOnlyRestCollection<TEntity, TKey>
        where TEntity : class, IEntityBase<TKey>
        where TKey : IEquatable<TKey>
    {
        public ReadOnlyRestCollection(IReadOnlyRepository<TEntity, TKey> repository)
        {
            Repository = repository;
        }

        protected IReadOnlyRepository<TEntity, TKey> Repository { get; set; }

        public async Task<bool> AnyAsync(IQuery<TEntity> query)
        {
            query.Options.NeedEnumeration = false;
            query.Options.NeedCount = true;

            return (await GetAsync(query)).Count > 0;
        }

        public virtual Task<ISelection<TEntity>> GetAsync(IQuery<TEntity> query)
        {
            return Repository.GetAsync(query);
        }

        public virtual Task<TEntity> GetByIdAsync(TKey id, IQuery<TEntity> query)
        {
            return Repository.GetByIdAsync(id, query);
        }
    }
}