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
	public class Repository<TEntity> : IRepository<TEntity>
		where TEntity : class, IEntityBase
	{
		protected IStorageService _storageService;
		protected IExecutionContext _executionContext;
		protected ICombinationsHolder _combinationsHolder;

		public Repository(IStorageService storageService, IExecutionContext executionContext, ICombinationsHolder combinationsHolder)
		{
			_storageService = storageService;
			_executionContext = executionContext;
			_combinationsHolder = combinationsHolder;
		}

		public virtual Task<int> CountAsync(Query<TEntity> query)
		{
			var entities = Set(query);

			if (query.Options.NeedFilterRights)
			{
				entities = ApplyRights(entities, query);
			}

			entities = ApplyFilters(entities, query);

			return CountEntities(entities);
		}
		protected virtual Task<int> CountEntities(IQueryable<TEntity> entities)
		{
			return Task.FromResult<int>(entities.Count());
		}

		public virtual Task<IEnumerable<TEntity>> EnumerateAsync(Query<TEntity> query)
		{
			var entities = Set(query);

			if (query.Options.NeedFilterRights)
			{
				entities = ApplyRights(entities, query);
			}

			entities = ApplyFilters(entities, query);
			entities = ApplyOrderBys(entities, query);
			entities = ApplyPage(entities, query);
			entities = ApplyIncludes(entities, query);

			return EnumerateEntities(entities);
		}
		protected virtual Task<IEnumerable<TEntity>> EnumerateEntities(IQueryable<TEntity> entities)
		{
			return Task.FromResult<IEnumerable<TEntity>>(entities.ToList());
		}

		public virtual void Add(TEntity entity)
		{
			_storageService.Add(entity);
		}

		public virtual void AddRange(IEnumerable<TEntity> entities)
		{
			_storageService.AddRange(entities);
		}

		public virtual void Remove(TEntity entity)
		{
			_storageService.Remove(entity);
		}

		protected virtual IQueryable<TEntity> Set(Query<TEntity> query)
		{
			return _storageService.Set<TEntity>();
		}

		protected virtual IQueryable<TEntity> ApplyRights(IQueryable<TEntity> entities, Query<TEntity> query)
		{
			var operationIds = GetOperationIds(query.Verb);
			if (!operationIds.Any())
			{
				throw new UnreachableEntityTypeException<TEntity>();
			}
			
			return _executionContext.curPrincipal
				.ApplyRights(entities, operationIds);
		}
		protected virtual IQueryable<TEntity> ApplyFilters(IQueryable<TEntity> entities, Query<TEntity> query)
		{
			return entities.Where(query.FiltersAsExpression());
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

		protected virtual HashSet<int> GetOperationIds(HttpVerb verb)
		{
			var combinations = _combinationsHolder.Combinations.Where(c => c.Subject == typeof(TEntity) && c.Verb == verb);

			return new HashSet<int>(combinations.Select(c => c.Operation.Id));
		}
	}
}
