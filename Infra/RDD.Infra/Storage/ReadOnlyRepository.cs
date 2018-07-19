using RDD.Domain;
using RDD.Domain.Models.Querying;
using RDD.Domain.Models.Querying.Convertors;
using RDD.Domain.Rights;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RDD.Infra.Storage
{
    public class ReadOnlyRepository<TEntity> : IReadOnlyRepository<TEntity>
        where TEntity : class
    {
        protected IStorageService _storageService;
        protected IRightsService _rightsService;

        public ReadOnlyRepository(IStorageService storageService, IRightsService rightsService)
        {
            _storageService = storageService;
            _rightsService = rightsService;
        }

        public virtual Task<int> CountAsync()
        {
            return CountAsync(new Query<TEntity>());
        }
        public virtual Task<int> CountAsync(Query<TEntity> query)
        {
            var entities = Set(query);

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
            var entities = Set(query);

            if (query.Options.CheckRights)
            {
                entities = ApplyRights(entities, query);
            }

            entities = ApplyFilters(entities, query);
            entities = ApplyOrderBys(entities, query);
            entities = ApplyPage(entities, query);
            entities = ApplyIncludes(entities, query);

            return _storageService.EnumerateEntitiesAsync(entities);
        }

        public virtual Task<IEnumerable<TEntity>> PrepareAsync(IEnumerable<TEntity> entities, Query<TEntity> query)
        {
            return Task.FromResult(entities);
        }

        protected virtual IQueryable<TEntity> Set(Query<TEntity> query)
        {
            return _storageService.Set<TEntity>();
        }

        protected virtual IQueryable<TEntity> ApplyRights(IQueryable<TEntity> entities, Query<TEntity> query)
        {
            return entities.Where(_rightsService.GetFilter(query));
        }
        protected virtual IQueryable<TEntity> ApplyFilters(IQueryable<TEntity> entities, Query<TEntity> query)
        {
            return entities.Where(query.Filter.Expression);
        }
        protected virtual IQueryable<TEntity> ApplyOrderBys(IQueryable<TEntity> entities, Query<TEntity> query)
        {
            if (!query.OrderBys.Any())
            {
                return entities;
            }

            var orderBysConverter = new OrderBysConverter<TEntity>();

            return orderBysConverter.Convert(entities, query.OrderBys);
        }
        protected virtual IQueryable<TEntity> ApplyPage(IQueryable<TEntity> entities, Query<TEntity> query)
        {
            return entities.Skip(query.Page.Offset).Take(query.Page.Limit);
        }
        protected virtual IQueryable<TEntity> ApplyIncludes(IQueryable<TEntity> entities, Query<TEntity> query)
        {
            return entities;
        }
    }
}
