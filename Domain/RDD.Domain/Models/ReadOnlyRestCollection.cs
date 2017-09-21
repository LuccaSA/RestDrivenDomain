﻿using RDD.Domain.Exceptions;
using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace RDD.Domain.Models
{
	public class ReadOnlyRestCollection<TEntity, TKey> : IReadOnlyRestCollection<TEntity, TKey>
		where TEntity : class, IEntityBase<TEntity, TKey>
		where TKey : IEquatable<TKey>
	{
		protected IRepository<TEntity> _repository;
		protected IExecutionContext _execution;
		protected ICombinationsHolder _combinationsHolder;

		protected ReadOnlyRestCollection() { }
		public ReadOnlyRestCollection(IRepository<TEntity> repository, IExecutionContext execution, ICombinationsHolder combinationsHolder)
		{
			_repository = repository;
			_execution = execution;
			_combinationsHolder = combinationsHolder;
		}

		protected void AttachOperationsToEntity(TEntity entity)
		{
			AttachOperationsToEntities(new List<TEntity> { entity });
		}
		private void AttachOperationsToEntities(IEnumerable<TEntity> entities)
		{
			var operationsForAttach = new List<Operation>();//TODO  _appInstance.GetAllOperations<TEntity>();

			_execution.queryWatch.Start();

			AttachOperations(entities, operationsForAttach);

			_execution.queryWatch.Stop();
		}

		/// <summary>
		/// On ne filtre qu'en écriture, pas en lecture
		/// </summary>
		/// <returns></returns>
		protected virtual Query<TEntity> FilterRights(Query<TEntity> query, HttpVerb verb)
		{
			if (verb == HttpVerb.GET)
				return query;

			var operationIds = GetOperationIds(query, verb);
			if (!operationIds.Any())
			{
				throw new UnreachableEntityTypeException<TEntity>();
			}
			if (!_execution.curPrincipal.HasAnyOperations(operationIds))
			{
				throw new UnauthorizedException(String.Format("You do not have sufficient permission to make a {0} on type {1}", verb, typeof(TEntity).Name));
			}

			return query;
		}

		protected virtual void AttachOperations(IEnumerable<TEntity> entities, List<Operation> operations)
		{
			//TODO
			//if (operations.Any())
			//{
			//	var ops = ExecutionContext.Current.curPrincipal.GetOperations(_storage, _appInstance, new HashSet<int>(operations.Select(o => o.Id)));
			//	SetOperationsOnEntities(entities, entities.ToDictionary(o => o.Id, o => ops), operations);
			//}
		}

		protected void SetOperationsOnEntities(ICollection<TEntity> list, Dictionary<TKey, HashSet<int>> entityPerms, List<Operation> operations)
		{
			foreach (var el in list)
			{
				if (entityPerms.ContainsKey(el.Id))
				{
					el.AuthorizedOperations = operations.Where(op => entityPerms[el.Id].Contains(op.Id)).ToList();
				}
			}
		}

		/// <summary>
		/// Permet d'attacher des actions personnalisées en complément des opérations
		/// </summary>
		/// <param name="list"></param>
		internal virtual void AttachActions(IEnumerable<TEntity> list) { }
		protected void AttachActionsToEntity(TEntity entity)
		{
			AttachActionsToEntities(new HashSet<TEntity> { entity });
		}
		private void AttachActionsToEntities(IEnumerable<TEntity> list)
		{
			_execution.queryWatch.Start();

			AttachActions(list);

			_execution.queryWatch.Stop();
		}

		/// <summary>
		/// When a custom action needs to access the entities operations
		/// </summary>
		/// <param name="entities"></param>
		internal virtual void AppendOperationsToEntities(ICollection<TEntity> entities)
		{
			//TODO
			//var operationsForAttach = _appInstance.GetAllOperations<TEntity>();
			//AttachOperations(entities, operationsForAttach);
		}

		public async Task<bool> AnyAsync(HttpVerb verb)
		{
			//Le Count() C# est plus rapide qu'un Any() SQL
			return (await GetAsync(new Query<TEntity> { Verb = verb, Options = new Options { NeedEnumeration = false, NeedCount = true } })).Count > 0;
		}
		public async Task<bool> AnyAsync(Query<TEntity> query)
		{
			return (await GetAsync(query)).Count > 0;
		}

		public async Task<IEnumerable<TEntity>> GetAllAsync()
		{
			return (await GetAsync(new Query<TEntity>())).Items;
		}

		public async virtual Task<ISelection<TEntity>> GetAsync(Query<TEntity> query)
		{
			var count = 0;
			IEnumerable<TEntity> items = new HashSet<TEntity>();

			//En général on veut une énumération des entités
			if (query.Options.NeedEnumeration)
			{
				items = await _repository.EnumerateAsync(query);

				count = items.Count();

				//Si y'a plus d'items que le paging max ou que l'offset du paging n'est pas à 0, il faut compter la totalité des entités
				if (query.Page.Offset > 0 || query.Page.Limit <= count)
				{
					var countQuery = new Query<TEntity>(query) { Page = new UnlimitedPage() };

					count = await _repository.CountAsync(countQuery);
				}

				query.Page.TotalCount = count;

				//Si on a demandé les permissions, on va les chercher après énumération
				if (query.Options.AttachOperations)
				{
					AttachOperationsToEntities(items);
				}

				items = await PrepareAsync(items, query);

				//ON attache les actions après le Prepare, histoire que les objets soient le plus complets possibles
				if (query.Options.AttachActions)
				{
					AttachActionsToEntities(items);
				}
			}

			//Si c'était un PUT/DELETE, on en profite pour affiner la réponse
			if (query.Verb != HttpVerb.GET && count == 0 && items.Count() == 0 && await AnyAsync(query.Verb))
			{
				throw new NotFoundException(String.Format("No item of type {0} matching URL criteria while trying a {1}", typeof(TEntity).Name, query.Verb));
			}

			return new Selection<TEntity>(items, count);
		}

		public async Task<object> TryGetByIdAsync(object id)
		{
			try
			{
				return await GetByIdAsync((TKey)id);
			}
			catch { return null; }
		}
		public async Task<IEnumerable<object>> TryGetByIdsAsync(IEnumerable<object> id)
		{
			try
			{
				return (await GetByIdsAsync(new List<TKey>(id.Cast<TKey>()))).Cast<object>();
			}
			catch { return null; }
		}

		public Task<TEntity> GetByIdAsync(TKey id, HttpVerb verb = HttpVerb.GET)
		{
			return GetByIdAsync(id, new Query<TEntity>() { Verb = verb });
		}

		/// <summary>
		/// Si on ne trouve pas l'entité, on renvoie explicitement un NotFound
		/// puisque c'était explicitement cette entité qui était visée
		/// NB : on ne sait pas si l'entité existe mais qu'on n'y a pas accès ou si elle n'existe pas, mais c'est logique
		/// </summary>
		/// <param name="query"></param>
		/// <param name="id"></param>
		/// <param name="verb"></param>
		/// <returns></returns>
		public async virtual Task<TEntity> GetByIdAsync(TKey id, Query<TEntity> query)
		{
			var result = (await GetByIdsAsync(new List<TKey> { id }, query)).FirstOrDefault();

			if (result == null)
			{
				//NB : si verb == PUT alors l'exception UnAuthorized sera levée lors du GetByIds
				throw new NotFoundException(String.Format("Resource with ID {0} not found", id));
			}

			return result;
		}

		public async Task<IEnumerable<TEntity>> GetByIdsAsync(IList<TKey> ids, HttpVerb verb = HttpVerb.GET)
		{
			return await GetByIdsAsync(ids, new Query<TEntity> { Verb = verb });
		}
		public async virtual Task<IEnumerable<TEntity>> GetByIdsAsync(IList<TKey> ids, Query<TEntity> query)
		{
			query.Filters.Add(new Filter<TEntity>(new PropertySelector<TEntity>(e => e.Id), FilterOperand.Equals, (IList)ids));

			return (await GetAsync(query)).Items;
		}

		public virtual PropertySelector<TEntity> HandleIncludes(PropertySelector<TEntity> includes, HttpVerb verb, Field<TEntity> fields)
		{
			//On n'inclut pas les propriétés qui ne viennent pas de la BDD
			includes.Remove(t => t.AuthorizedOperations);
			//includes = helper.Remove(includes, t => t.Culture);
			//includes = helper.Remove(includes, t => t.Application);

			return includes;
		}
		/// <summary>
		/// Permet d'automatiser l'include des fields d'une propriété (sous entité) d'un TEntity
		/// </summary>
		/// <param name="includes">Les includes actuels auxquels il faut ajouter les includes de la sous entité</param>
		/// <param name="verb">Le verbe HTTP joué</param>
		/// <param name="fieldName">Le nom du field de l'entité qui représente la sous entité (~nom de la propriété)</param>
		/// <param name="subs">Les Fields de la sous entités = un sous ensemble des Fields du TEntity</param>
		/// <param name="appInstance">Si la sous entité est gérée par la même application que l'entité principale, on transmet l'appInstance, sinon ça n'est pas nécessaire</param>
		/// <returns>Les includes augmentés des includes de la sous entité</returns>
		protected PropertySelector<TEntity> HandleSubIncludes<TSubEntity, TSubKey>(PropertySelector<TEntity> includes, HttpVerb verb, Field<TEntity> fields, LambdaExpression selector, IRestCollection<TSubEntity> subs)
			where TSubEntity : class, IEntityBase<TSubEntity, TSubKey>, new()
			where TSubKey : IEquatable<TSubKey>
		{
			//On va d'abord chercher le Repo qui gère la sous entité
			var subFields = fields.TransfertTo<TSubEntity>(selector);
			var subIncludes = subs.HandleIncludes(new PropertySelector<TSubEntity>(), verb, subFields);

			if (!subIncludes.IsEmpty)
			{
				includes.Add<TSubEntity>(subIncludes, selector);
			}

			return includes;
		}
		protected PropertySelector<TEntity> HandleSubIncludes<TSubEntity, TSubKey>(PropertySelector<TEntity> includes, HttpVerb verb, Field<TEntity> fields, Expression<Func<TEntity, TSubEntity>> selector, IRestCollection<TSubEntity> subs)
			where TSubEntity : class, IEntityBase<TSubEntity, TSubKey>, new()
			where TSubKey : IEquatable<TSubKey>
		{
			return HandleSubIncludes<TSubEntity, TSubKey>(includes, verb, fields, (LambdaExpression)selector, subs);
		}
		protected PropertySelector<TEntity> HandleSubIncludes<TSubEntity, TSubKey>(PropertySelector<TEntity> includes, HttpVerb verb, Field<TEntity> fields, Expression<Func<TEntity, IEnumerable<TSubEntity>>> selector, IRestCollection<TSubEntity> subs)
			where TSubEntity : class, IEntityBase<TSubEntity, TSubKey>, new()
			where TSubKey : IEquatable<TSubKey>
		{
			return HandleSubIncludes<TSubEntity, TSubKey>(includes, verb, fields, (LambdaExpression)selector, subs);
		}

		//public IQueryable<TEntity> Includes(IQueryable<TEntity> entities)
		//{
		//	return Includes(entities, new Query<TEntity>(), HttpVerb.GET, new Field<TEntity>());
		//}

		//public virtual IQueryable<TEntity> Includes(IQueryable<TEntity> entities, Query<TEntity> query, HttpVerb verb, Field<TEntity> fields)
		//{
		//	if (query != null)
		//	{
		//		query.Includes = HandleIncludes(query.Includes, verb, fields);

		//		return _storage.Includes<TEntity>(entities, query.Includes);
		//	}
		//	return entities;
		//}

		protected virtual HashSet<int> GetOperationIds(Query<TEntity> query, HttpVerb verb)
		{
			var combinations = _combinationsHolder.Combinations.Where(c => c.Subject == typeof(TEntity) && c.Verb == verb);

			return new HashSet<int>(combinations.Select(c => c.Operation.Id));
		}

		public virtual Task<IEnumerable<TEntity>> PrepareAsync(IEnumerable<TEntity> entities, Query<TEntity> query)
		{
			return Task.FromResult(entities);
		}
	}
}