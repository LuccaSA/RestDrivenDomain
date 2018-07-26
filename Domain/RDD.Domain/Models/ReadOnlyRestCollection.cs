using RDD.Domain.Exceptions;
using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RDD.Domain.Models
{
    public class ReadOnlyRestCollection<TEntity, TKey> : IReadOnlyRestCollection<TEntity, TKey>
        where TEntity : class, IEntityBase<TKey>
        where TKey : IEquatable<TKey>
    {
        private readonly QueryContext _queryContext;

        public ReadOnlyRestCollection(IReadOnlyRepository<TEntity> repository, QueryContext queryContext)
        {
            _queryContext = queryContext;
            Repository = repository;
        }

        protected IReadOnlyRepository<TEntity> Repository { get; set; }

        public async Task<bool> AnyAsync(Query<TEntity> query)
        {
            _queryContext.Request.NeedEnumeration = false;
            _queryContext.Request.NeedCount = true;

            await GetAsync(query);
            return _queryContext.Response.TotalCount > 0;
        }

        public async Task<IReadOnlyCollection<TEntity>> GetAllAsync() => (await GetAsync(new Query<TEntity>()));

        public virtual async Task<IReadOnlyCollection<TEntity>> GetAsync(Query<TEntity> query)
        {
            var totalCount = -1;
            IReadOnlyCollection<TEntity> items = null;

            //Dans de rares cas on veut seulement le count des entités
            if (_queryContext.Request.NeedCount && !_queryContext.Request.NeedEnumeration)
            {
                _queryContext.Response.TotalCount = totalCount = await Repository.CountAsync(query);
            }

            //En général on veut une énumération des entités
            if (_queryContext.Request.NeedEnumeration)
            {
                items = await Repository.GetAsync(query);
                _queryContext.Response.TotalCount = totalCount != -1 ? totalCount : await Repository.CountAsync(query);
                items = await Repository.PrepareAsync(items, query);
            }

            return items ?? new List<TEntity>();
        }

        public virtual async Task<TEntity> GetByIdAsync(TKey id, Query<TEntity> query)
        {
            TEntity result = (await GetAsync(new Query<TEntity>(query, e => e.Id.Equals(id)))).FirstOrDefault();

            return result;
        }

        protected Task<bool> AnyAsync() => AnyAsync(new Query<TEntity>());
    }
}