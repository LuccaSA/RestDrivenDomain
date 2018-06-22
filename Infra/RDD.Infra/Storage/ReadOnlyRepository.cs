using RDD.Domain;
using RDD.Domain.Exceptions;
using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying;
using RDD.Domain.Models.Querying.Convertors;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RDD.Infra.Storage
{
    public class ReadOnlyRepository<TEntity> : IReadOnlyRepository<TEntity>
        where TEntity : class
    {
        protected IStorageService StorageService { get; }
        protected IExecutionContext ExecutionContext { get; }
        protected ICombinationsHolder CombinationsHolder { get; }

        public ReadOnlyRepository(IStorageService storageService, IExecutionContext executionContext, ICombinationsHolder combinationsHolder)
        {
            StorageService = storageService;
            ExecutionContext = executionContext;
            CombinationsHolder = combinationsHolder;
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

            return StorageService.EnumerateEntitiesAsync(entities);
        }

        public virtual Task<IEnumerable<TEntity>> PrepareAsync(IEnumerable<TEntity> entities)
        {
            return PrepareAsync(entities, new Query<TEntity>());
        }
        public virtual Task<IEnumerable<TEntity>> PrepareAsync(IEnumerable<TEntity> entities, Query<TEntity> query)
        {
            return Task.FromResult(entities);
        }

        protected virtual IQueryable<TEntity> Set(Query<TEntity> query)
        {
            return StorageService.Set<TEntity>();
        }

        protected virtual IQueryable<TEntity> ApplyRights(IQueryable<TEntity> entities, Query<TEntity> query)
        {
            var operationIds = GetOperationIds(query.Verb);
            if (!operationIds.Any())
            {
                throw new UnreachableEntityException(typeof(TEntity));
            }
            
            return ExecutionContext.curPrincipal
                .ApplyRights(entities, operationIds);
        }
        protected virtual IQueryable<TEntity> ApplyFilters(IQueryable<TEntity> entities, Query<TEntity> query)
        {
            return entities.Where(query.Filters);
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

        protected virtual HashSet<int> GetOperationIds(HttpVerbs verb)
        {
            var combinations = CombinationsHolder.Combinations.Where(c => c.Subject == typeof(TEntity) && c.Verb.HasVerb(verb));

            return new HashSet<int>(combinations.Select(c => c.Operation.Id));
        }
    }
}
