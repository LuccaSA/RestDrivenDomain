using Microsoft.EntityFrameworkCore;
using Rdd.Domain;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Domain.Models.Querying;
using Rdd.Domain.Rights;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rdd.Infra.Storage
{
    public class ReadOnlyRepository<TEntity> : IReadOnlyRepository<TEntity>
        where TEntity : class
    {
        protected IStorageService StorageService { get; set; }
        protected IRightExpressionsHelper<TEntity> RightExpressionsHelper { get; set; }
        protected virtual IExpressionTree IncludeWhiteList { get; }
        private readonly IIncludeApplicator _includeApplicator;

        public ReadOnlyRepository(IStorageService storageService, IRightExpressionsHelper<TEntity> rightExpressionsHelper, IIncludeApplicator includeApplicator)
        {
            StorageService = storageService;
            RightExpressionsHelper = rightExpressionsHelper;
            _includeApplicator = includeApplicator;
        }

        public virtual async Task<int> CountAsync(Query<TEntity> query)
        {
            IQueryable<TEntity> entities = Set();

            if (query.Options.ChecksRights)
            {
                entities = await ApplyRightsAsync(entities, query);
            }

            entities = ApplyFilters(entities, query);

            return await CountEntitiesAsync(entities);
        }

        protected virtual Task<int> CountEntitiesAsync(IQueryable<TEntity> entities)
            => StorageService.CountAsync(entities);

        public virtual async Task<IEnumerable<TEntity>> GetAsync(Query<TEntity> query)
        {
            IQueryable<TEntity> entities = Set();

            entities = ApplyDataTracking(entities, query);

            if (query.Options.ChecksRights)
            {
                entities = await ApplyRightsAsync(entities, query);
            }

            entities = ApplyFilters(entities, query);
            entities = ApplyOrderBys(entities, query);

            if (query.Page != Page.Unlimited)
            {
                entities = ApplyPage(entities, query);
            }

            entities = ApplyIncludes(entities, query);

            //last as to allow for type conditional includes
            entities = ApplyTypeFilter(entities, query);

            // Entry point to prefetch data before enumeration
            entities = await BeforeEnumerateAsync(entities, query);

            return await StorageService.EnumerateEntitiesAsync(entities);
        }

        protected virtual Task<IQueryable<TEntity>> BeforeEnumerateAsync(IQueryable<TEntity> entities, Query<TEntity> query) => Task.FromResult(entities);

        public virtual Task<IEnumerable<TEntity>> PrepareAsync(IEnumerable<TEntity> entities, Query<TEntity> query)
        {
            return Task.FromResult(entities);
        }

        public async Task<bool> AnyAsync(Query<TEntity> query)
        {
            IQueryable<TEntity> entities = Set();

            if (query.Options.ChecksRights)
            {
                entities = await ApplyRightsAsync(entities, query);
            }

            entities = ApplyFilters(entities, query);

            return await StorageService.AnyAsync(entities);
        }

        protected virtual IQueryable<TEntity> Set() => StorageService.Set<TEntity>();

        protected virtual async Task<IQueryable<TEntity>> ApplyRightsAsync(IQueryable<TEntity> entities, Query<TEntity> query)
        {
            var filter = await RightExpressionsHelper.GetFilterAsync(query);
            return entities.Where(filter);
        }

        protected virtual IQueryable<TEntity> ApplyFilters(IQueryable<TEntity> entities, Query<TEntity> query)
        {
            if (query.Filter == null)
            {
                return entities;
            }
            return entities.Where(query.Filter.Expression);
        }

        protected virtual IQueryable<TEntity> ApplyTypeFilter(IQueryable<TEntity> entities, Query<TEntity> query)
        {
            if (query.TypeFilter != null)
            {
                return query.TypeFilter.Apply(entities);
            }
            return entities;
        }

        protected virtual IQueryable<TEntity> ApplyOrderBys(IQueryable<TEntity> entities, Query<TEntity> query)
        {
            foreach (var orderBy in query.OrderBys)
            {
                entities = orderBy.ApplyOrderBy(entities);
            }

            return entities;
        }

        protected virtual IQueryable<TEntity> ApplyPage(IQueryable<TEntity> entities, Query<TEntity> query)
        {
            return entities.Skip(query.Page.Offset).Take(query.Page.Limit);
        }

        protected virtual IQueryable<TEntity> ApplyIncludes(IQueryable<TEntity> entities, Query<TEntity> query) 
            => _includeApplicator.ApplyIncludes<TEntity>(entities, query, IncludeWhiteList);

        protected virtual IQueryable<TEntity> ApplyDataTracking(IQueryable<TEntity> entities, Query<TEntity> query)
        {
            return query.Options.NeedsDataTracking ? entities : entities.AsNoTracking();
        }
    }
}