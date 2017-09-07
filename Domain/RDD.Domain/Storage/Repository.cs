using RDD.Domain.Exceptions;
using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying;
using RDD.Domain.Models.Querying.Convertors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain.Storage
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
			var result = QueryEntities(query).Count();

			return Task.FromResult<int>(result);
		}

		public virtual Task<IEnumerable<TEntity>> EnumerateAsync(Query<TEntity> query)
		{
			var result = QueryEntities(query).ToList();

			return Task.FromResult<IEnumerable<TEntity>>(result);
		}

		public virtual Task<IEnumerable<TEntity>> EnumerateAsync(ExpressionQuery<TEntity> query)
		{
			var result = QueryEntities(query).ToList();

			return Task.FromResult<IEnumerable<TEntity>>(result);
		}

		public virtual void Add(TEntity entity)
		{
			_storageService.Add(entity);
		}

		public virtual void Remove(TEntity entity)
		{
			_storageService.Remove(entity);
		}

		protected virtual IQueryable<TEntity> Set(Query<TEntity> query)
		{
			return _storageService.Set<TEntity>();
		}

		protected virtual IQueryable<TEntity> QueryEntities(Query<TEntity> query)
		{
			var result = Set(query);

			if (query.Options.NeedFilterRights)
			{
				result = ApplyRights(result, query);
			}

			result = ApplyFilters(result, query);
			result = ApplyOrderBys(result, query);

			return result;
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

		protected virtual HashSet<int> GetOperationIds(HttpVerb verb)
		{
			var combinations = _combinationsHolder.Combinations.Where(c => c.Subject == typeof(TEntity) && c.Verb == verb);

			return new HashSet<int>(combinations.Select(c => c.Operation.Id));
		}
	}
}
