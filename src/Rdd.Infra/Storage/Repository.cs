using Rdd.Domain;
using Rdd.Domain.Exceptions;
using Rdd.Domain.Helpers;
using Rdd.Domain.Models.Querying;
using Rdd.Domain.Rights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Rdd.Infra.Storage
{
    public class Repository<TEntity> : ReadOnlyRepository<TEntity>, IRepository<TEntity>
        where TEntity : class
    {
        public Repository(IStorageService storageService, IRightExpressionsHelper<TEntity> rightExpressionsHelper, IIncludeApplicator includeApplicator)
            : base(storageService, rightExpressionsHelper, includeApplicator) { }

        public virtual async Task AddAsync(TEntity entity, Query<TEntity> query)
        {
            if (query.Options.ChecksRights)
            {
                var filter = await RightExpressionsHelper.GetFilterAsync(query);
                await EnsureEntitiesAreLoaded(entity.Yield(), query, filter);
                var rightFilter = filter.Compile();
                if (!rightFilter(entity))
                {
                    throw new ForbiddenException("You do not have the rights to create the posted entity");
                }
            }

            StorageService.Add(entity);
        }

        public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, Query<TEntity> query)
        {
            if (query.Options.ChecksRights)
            {
                var filter = await RightExpressionsHelper.GetFilterAsync(query);
                await EnsureEntitiesAreLoaded(entities, query, filter);
                var rightFilter = filter.Compile();
                if (!entities.All(rightFilter))
                {
                    throw new ForbiddenException("You do not have the rights to create one of the posted entity");
                }
            }

            StorageService.AddRange(entities);
        }

        protected virtual Task EnsureEntitiesAreLoaded(IEnumerable<TEntity> entities, Query<TEntity> query, Expression<Func<TEntity, bool>> filter)
            => Task.CompletedTask;

        public virtual void Remove(TEntity entity)
        {
            StorageService.Remove(entity);
        }

        public void DiscardChanges(TEntity entity)
        {
            StorageService.DiscardChanges(entity);
        }
    }
}