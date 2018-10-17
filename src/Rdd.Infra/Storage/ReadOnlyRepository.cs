﻿using Microsoft.EntityFrameworkCore;
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

        public ReadOnlyRepository(IStorageService storageService, IRightExpressionsHelper<TEntity> rightExpressionsHelper)
        {
            StorageService = storageService;
            RightExpressionsHelper = rightExpressionsHelper;
        }

        public virtual Task<int> CountAsync() => CountAsync(new Query<TEntity>());
        public virtual Task<int> CountAsync(Query<TEntity> query)
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

        public virtual Task<IEnumerable<TEntity>> GetAsync(Query<TEntity> query)
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

            return StorageService.EnumerateEntitiesAsync(entities);
        }

        public virtual Task<IEnumerable<TEntity>> PrepareAsync(IEnumerable<TEntity> entities, Query<TEntity> query)
        {
            return Task.FromResult(entities);
        }

        protected virtual IQueryable<TEntity> Set()
        {
            return StorageService.Set<TEntity>();
        }

        protected virtual IQueryable<TEntity> ApplyRights(IQueryable<TEntity> entities, Query<TEntity> query)
        {
            return entities.Where(RightExpressionsHelper.GetFilter(query));
        }
        protected virtual IQueryable<TEntity> ApplyFilters(IQueryable<TEntity> entities, Query<TEntity> query)
        {
            if (query?.Filter?.Expression == null)
            {
                return entities;
            }
            return entities.Where(query.Filter.Expression);
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