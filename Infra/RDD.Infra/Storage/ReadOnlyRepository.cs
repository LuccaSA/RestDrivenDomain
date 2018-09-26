﻿using Microsoft.EntityFrameworkCore;
using RDD.Domain;
using RDD.Domain.Helpers.Expressions;
using RDD.Domain.Models.Querying;
using RDD.Domain.Rights;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RDD.Infra.Storage
{
    public class ReadOnlyRepository<TEntity> : IReadOnlyRepository<TEntity>
        where TEntity : class
    {
        protected IStorageService StorageService { get; set; }
        protected IRightExpressionsHelper<TEntity> RightExpressionsHelper { get; set; }

        protected virtual PropertyTreeNode IncludeWhiteList { get; }

        public ReadOnlyRepository(IStorageService storageService, IRightExpressionsHelper<TEntity> rightExpressionsHelper)
        {
            StorageService = storageService;
            RightExpressionsHelper = rightExpressionsHelper;
        }

        public virtual Task<int> CountAsync() => CountAsync(new Query<TEntity>());
        public virtual Task<int> CountAsync(Query<TEntity> query)
        {
            var entities = Set(query);

            if (query.CheckRights)
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

        public virtual Task<IReadOnlyCollection<TEntity>> GetAsync(Query<TEntity> query)
        {
            var entities = Set(query);

            if (query.CheckRights)
            {
                entities = ApplyRights(entities, query);
            }

            entities = ApplyFilters(entities, query);
            entities = ApplyOrderBys(entities, query);
            entities = ApplyPage(entities, query);
            entities = ApplyIncludes(entities, query);

            return StorageService.EnumerateEntitiesAsync(entities);
        }

        public virtual Task<IReadOnlyCollection<TEntity>> PrepareAsync(IEnumerable<TEntity> entities, Query<TEntity> query)
        {
            return Task.FromResult(entities is IReadOnlyCollection<TEntity> collection
                ? collection
                : entities?.ToList());
        }

        protected virtual IQueryable<TEntity> Set(Query<TEntity> query)
        {
            return StorageService.Set<TEntity>();
        }

        protected virtual IQueryable<TEntity> ApplyRights(IQueryable<TEntity> entities, Query<TEntity> query)
        {
            return entities.Where(RightExpressionsHelper.GetFilter(query));
        }
        protected virtual IQueryable<TEntity> ApplyFilters(IQueryable<TEntity> entities, Query<TEntity> query)
        {
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
            // Skip the skip/take paging step if unlimited to optimize sql query-plan
            if (query.Paging == QueryPaging.Unlimited)
            {
                return entities;
            }
            return entities
                .Skip(query.Paging.PageOffset * query.Paging.ItemPerPage)
                .Take(query.Paging.ItemPerPage);
        }

        protected virtual IQueryable<TEntity> ApplyIncludes(IQueryable<TEntity> entities, Query<TEntity> query)
        {
            if (IncludeWhiteList == null || query.Fields == null)
            {
                return entities;
            }

            var interect = query.Fields.Intersection(IncludeWhiteList);

            foreach (var path in interect.AsExpandedPaths<TEntity>())
            {
                entities = entities.Include(path);
            }

            return entities;

        }
    }
}