using LinqKit;
using Microsoft.EntityFrameworkCore;
using RDD.Domain.Exceptions;
using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace RDD.Domain.Models
{
	public partial class ReadOnlyRestCollection<TEntity, TKey> : IReadOnlyRestCollection<TEntity, TKey>
		where TEntity : class, IEntityBase<TEntity, TKey>
		where TKey : IEquatable<TKey>
	{
		protected IStorageService _storage;
		protected IExecutionContext _execution;
		protected ICombinationsHolder _combinationsHolder;
		protected Func<IStorageService> _asyncStorage;

		protected ReadOnlyRestCollection() { }
		public ReadOnlyRestCollection(IStorageService storage, IExecutionContext execution, ICombinationsHolder combinationsHolder, Func<IStorageService> asyncStorage = null)
		{
			_storage = storage;
			_execution = execution;
			_combinationsHolder = combinationsHolder;
			_asyncStorage = asyncStorage;
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
		internal protected IQueryable<TEntity> Set()
		{
			return Set(new Query<TEntity>());
		}
		internal protected virtual IQueryable<TEntity> Set(Query<TEntity> query)
		{
			return _storage.Set<TEntity>();
		}

		/// <summary>
		/// On ne filtre qu'en écriture, pas en lecture
		/// </summary>
		/// <returns></returns>
		protected virtual IQueryable<TEntity> FilterRights(IQueryable<TEntity> entities, Query<TEntity> query, HttpVerb verb)
		{
			if (verb == HttpVerb.GET)
				return entities;

			var operationIds = GetOperationIds(query, verb);
			if (!operationIds.Any())
			{
				throw new UnreachableEntityTypeException<TEntity>();
			}
			if (!_execution.curPrincipal.HasAnyOperations(_storage, operationIds))
			{
				throw new UnauthorizedException(String.Format("You do not have sufficient permission to make a {0} on type {1}", verb, typeof(TEntity).Name));
			}

			return entities;
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

		public async Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>> filter, HttpVerb verb = HttpVerb.GET)
		{
			return (await GetAsync(new Query<TEntity> { ExpressionFilters = filter }, verb)).Items;
		}
		public async Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, object>> field, HttpVerb verb = HttpVerb.GET)
		{
			return (await GetAsync(new Query<TEntity>(field, true) { ExpressionFilters = filter }, verb)).Items;
		}

		public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> filter)
		{
			//Le Count() C# est plus rapide qu'un Any() SQL
			return (await GetAsync(new Query<TEntity> { ExpressionFilters = filter, Options = new Options { NeedEnumeration = false, NeedCount = true } }, HttpVerb.GET)).Count > 0;
		}

		public async Task<IEnumerable<TEntity>> GetAllAsync()
		{
			return (await GetAsync(new Query<TEntity>(), HttpVerb.GET)).Items;
		}

		public async virtual Task<ISelection<TEntity>> GetAsync(Query<TEntity> query, HttpVerb verb = HttpVerb.GET)
		{
			return await GetAsync(Set(query), query, verb);
		}

		protected async virtual Task<ISelection<TEntity>> GetAsync(IQueryable<TEntity> entities, Query<TEntity> query, HttpVerb verb = HttpVerb.GET)
		{
			var count = 0;
			IEnumerable<TEntity> items = new HashSet<TEntity>();

			//On filtre les entités selon celles que l'on peut voir
			if (query.Options.NeedFilterRights)
			{
				entities = FilterRights(entities, query, verb);
			}

			//On joue les Wheres
			entities = ApplyFilters(entities, query);

			//On mémorise le fait qu'on fait un Count SQl ou C#
			var sqlCount = (query.Options.NeedCount && (!query.Options.NeedEnumeration || query.Options.withPagingInfo));

			//Dans de rares cas on veut seulement, ou en plus, le count des entités
			//On le fait en SQL si et seulement si y'a pas d'énumération (auquel cas on le fait en C#)
			//Ou qu'il y a énumération avec du paging (en cas de paging, le count doit compter TOUTES les entités et pas juste celles paginées
			if (sqlCount)
			{
				_execution.queryWatch.Start();

				count = await entities.CountAsync();

				_execution.queryWatch.Stop();
			}

			//En général on veut une énumération des entités
			if (query.Options.NeedEnumeration)
			{
				//Les orderby
				entities = ApplyOrderBys(entities, query);

				//Paging => BUG dans EF sur le paging ?!!
				if (query.Options.withPagingInfo)
				{
					entities = entities.Skip(query.Options.Page.Offset).Take(query.Options.Page.Limit);
				}

				entities = Includes(entities, query, verb, query.Fields);

				_execution.queryWatch.Start();

				items = await entities.ToListAsync();

				_execution.queryWatch.Stop();

				//Ici on a énuméré, y'a pas eu de paging mais on veut le count, donc il n'a pas été fait en SQL, faut le faire en C#
				if (query.Options.NeedCount && !sqlCount)
				{
					count = items.Count();
				}

				//Si pagination, alors on met à jour le count total
				if (query.Options.Page != null)
				{
					query.Options.Page.TotalCount = count;
				}

				//Si on a demandé les permissions, on va les chercher après énumération
				if (query.Options.attachOperations)
				{
					AttachOperationsToEntities(items);
				}

				items = await PrepareAsync(items, query);

				//ON attache les actions après le Prepare, histoire que les objets soient le plus complets possibles
				if (query.Options.attachActions)
				{
					AttachActionsToEntities(items);
				}
			}

			//Si c'était un PUT/DELETE, on en profite pour affiner la réponse
			if (verb != HttpVerb.GET && count == 0 && items.Count() == 0 && await AnyAsync(i => true))
			{
				throw new NotFoundException(String.Format("No item of type {0} matching URL criteria while trying a {1}", typeof(TEntity).Name, verb));
			}

			return new Selection<TEntity>(items, count);
		}

		public async Task<object> TryGetByIdAsync(object id, HttpVerb verb = HttpVerb.GET)
		{
			try
			{
				return await GetByIdAsync((TKey)id, verb);
			}
			catch { return null; }
		}
		public async Task<IEnumerable<object>> TryGetByIdsAsync(IEnumerable<object> id, HttpVerb verb = HttpVerb.GET)
		{
			try
			{
				return (await GetByIdsAsync(new HashSet<TKey>(id.Cast<TKey>()), verb)).Cast<object>();
			}
			catch { return null; }
		}

		public async Task<TEntity> GetByIdAsync(TKey id, HttpVerb verb = HttpVerb.GET)
		{
			return await GetByIdAsync(id, new Query<TEntity>() { Verb = verb }, verb);
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
		public async virtual Task<TEntity> GetByIdAsync(TKey id, Query<TEntity> query, HttpVerb verb = HttpVerb.GET)
		{
			var result = (await GetByIdsAsync(new HashSet<TKey> { id }, query, verb)).FirstOrDefault();

			if (result == null)
			{
				//NB : si verb == PUT alors l'exception UnAuthorized sera levée lors du GetByIds
				throw new NotFoundException(String.Format("Resource with ID {0} not found", id));
			}

			return result;
		}

		public async Task<IEnumerable<TEntity>> GetByIdsAsync(ISet<TKey> ids, HttpVerb verb = HttpVerb.GET)
		{
			return await GetByIdsAsync(ids, new Query<TEntity>(), verb);
		}
		public async virtual Task<IEnumerable<TEntity>> GetByIdsAsync(ISet<TKey> ids, Query<TEntity> query, HttpVerb verb = HttpVerb.GET)
		{
			query.ExpressionFilters = Equals("id", ids.ToList()).Expand();
			return (await GetAsync(query, verb)).Items;
		}

		private IQueryable<TEntity> ApplyOrderBys(IQueryable<TEntity> entities, Query<TEntity> query)
		{
			//OrderBy => si y'a pas de orderby, il faut en mettre 1 par défaut, sinon le Skip plante
			if (query.OrderBys.Count == 0)
			{
				if (query.Options.withPagingInfo)
				{
					entities = OrderByDefault(entities);
				}
			}
			else
			{
				for (int i = 0, length = query.OrderBys.Count; i < length; i++)
				{
					entities = OrderBy(entities, query.OrderBys[i].Field, query.OrderBys[i].Direction, i == 0);
				}
			}

			return entities;
		}
		protected virtual IQueryable<TEntity> ApplyFilters(IQueryable<TEntity> entities, Query<TEntity> query)
		{
			var expressionFilters = query.ExpressionFilters; //Filtres C#
			var queryFilters = query.Filters; //Filtre via la couche web

			//Si y'a des filtres
			if (expressionFilters != null || queryFilters != null)
			{
				Expression<Func<TEntity, bool>> filters;

				//Soit via C#
				if (expressionFilters != null)
				{
					filters = expressionFilters;
				}
				else //Soit via l'URL
				{
					var predicate = new PredicateService(queryFilters);
					filters = predicate.GetEntityPredicate<TEntity, TKey>(this);
				}

				//On joue les wheres
				entities = entities.Where(filters);
			}

			return entities;
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

		public IQueryable<TEntity> Includes(IQueryable<TEntity> entities)
		{
			return Includes(entities, new Query<TEntity>(), HttpVerb.GET, new Field<TEntity>());
		}

		public virtual IQueryable<TEntity> Includes(IQueryable<TEntity> entities, Query<TEntity> query, HttpVerb verb, Field<TEntity> fields)
		{
			if (query != null)
			{
				query.Includes = HandleIncludes(query.Includes, verb, fields);

				return _storage.Includes<TEntity>(entities, query.Includes);
			}
			return entities;
		}

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
