using Microsoft.EntityFrameworkCore;
using Rdd.Domain;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Domain.Models;
using Rdd.Domain.Models.Querying;
using Rdd.Domain.Rights;
using Rdd.Infra.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rdd.Infra.Storage
{
    public class ReadOnlyRepository<TEntity, TKey> : IReadOnlyRepository<TEntity, TKey>
        where TEntity : class, IPrimaryKey<TKey>
        where TKey : IEquatable<TKey>
    {
        protected IStorageService StorageService { get; set; }
        protected IRightExpressionsHelper<TEntity> RightExpressionsHelper { get; set; }

        protected virtual IExpressionTree IncludeWhiteList { get; }

        public ReadOnlyRepository(IStorageService storageService, IRightExpressionsHelper<TEntity> rightExpressionsHelper)
        {
            StorageService = storageService;
            RightExpressionsHelper = rightExpressionsHelper;
        }

        public virtual Task<int> CountAsync() => CountAsync(new Query<TEntity>());
        public virtual Task<int> CountAsync(IQuery<TEntity> query)
        {
            var entities = Set();

            if (query.Options.CheckRights)
            {
                entities = ApplyRights(entities, query);
            }

            entities = ApplyFilters(entities, query);

            return CountEntities(entities);
        }
        protected virtual Task<int> CountEntities(IQueryable<TEntity> entities)
        {
            return Task.FromResult(entities.Count());
        }

        public virtual async Task<TEntity> GetByIdAsync(TKey id, IQuery<TEntity> query)
        {
            var clone = new Query<TEntity>(query, e => e.Id.Equals(id));

            return (await GetAsync(clone)).Items.FirstOrDefault();
        }

        public virtual async Task<IEnumerable<TEntity>> GetByIdsAsync(IEnumerable<TKey> ids, IQuery<TEntity> query)
        {
            var clone = new Query<TEntity>(query, e => ids.Contains(e.Id));

            return (await GetAsync(clone)).Items;
        }
        public virtual async Task<ISelection<TEntity>> GetAsync(IQuery<TEntity> query)
        {
            var count = 0;
            IEnumerable<TEntity> items = new HashSet<TEntity>();

            //Dans de rares cas on veut seulement le count des entités
            if (query.Options.NeedCount && !query.Options.NeedEnumeration)
            {
                count = await CountAsync(query);
            }

            //En général on veut une énumération des entités
            if (query.Options.NeedEnumeration)
            {
                var entities = Set();

                if (query.Options.CheckRights)
                {
                    entities = ApplyRights(entities, query);
                }

                entities = ApplyFilters(entities, query);
                entities = ApplyOrderBys(entities, query);

                if (query.Page != Page.Unlimited)
                {
                    entities = ApplyPage(entities, query);
                }

                entities = ApplyIncludes(entities, query);

                items = await StorageService.EnumerateEntitiesAsync(entities);

                count = items.Count();

                //Si y'a plus d'items que le paging max ou que l'offset du paging n'est pas à 0, il faut compter la totalité des entités
                if (query.Page.Offset > 0 || query.Page.Limit <= count)
                {
                    count = await CountAsync(query);
                }

                items = await PrepareAsync(items, query);
            }

            return new Selection<TEntity>(items, count);
        }

        protected virtual Task<IEnumerable<TEntity>> PrepareAsync(IEnumerable<TEntity> entities, IQuery<TEntity> query)
        {
            return Task.FromResult(entities);
        }

        protected virtual IQueryable<TEntity> Set()
        {
            return StorageService.Set<TEntity>();
        }

        protected virtual IQueryable<TEntity> ApplyRights(IQueryable<TEntity> entities, IQuery<TEntity> query)
        {
            return entities.Where(RightExpressionsHelper.GetFilter(query));
        }
        protected virtual IQueryable<TEntity> ApplyFilters(IQueryable<TEntity> entities, IQuery<TEntity> query)
        {
            if (query?.Filter?.Expression == null)
            {
                return entities;
            }
            return entities.Where(query.Filter.Expression);
        }

        protected virtual IQueryable<TEntity> ApplyOrderBys(IQueryable<TEntity> entities, IQuery<TEntity> query)
        {
            foreach (var orderBy in query.OrderBys)
            {
                entities = orderBy.ApplyOrderBy(entities);
            }

            return entities;
        }
        protected virtual IQueryable<TEntity> ApplyPage(IQueryable<TEntity> entities, IQuery<TEntity> query)
        {
            return entities.Skip(query.Page.Offset).Take(query.Page.Limit);
        }

        protected virtual IQueryable<TEntity> ApplyIncludes(IQueryable<TEntity> entities, IQuery<TEntity> query)
        {
            if (IncludeWhiteList == null || query.Fields == null)
            {
                return entities;
            }

            foreach (var prop in query.Fields.Intersection(IncludeWhiteList))
            {
                entities = entities.Include(prop.Name);
            }

            return entities;

        }
    }
}