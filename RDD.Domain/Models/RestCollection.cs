using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using LinqKit;
using Newtonsoft.Json;
using RDD.Domain.Models.Querying;
using RDD.Domain.Helpers;
using RDD.Domain.Exceptions;

namespace RDD.Domain.Models
{
	public partial class RestCollection<TEntity, TKey> : IRestCollection<TEntity, TKey>
		where TEntity : class, IEntityBase<TEntity, TKey>, new()
		where TKey : IEquatable<TKey>
	{
		protected IStorageService _storage;
		protected IExecutionContext _execution;
		protected Func<IStorageService> _asyncStorage;

		protected RestCollection() { }
		public RestCollection(IStorageService storage, IExecutionContext execution, Func<IStorageService> asyncStorage = null)
		{
			_storage = storage;
			_execution = execution;
			_asyncStorage = asyncStorage;
		}

		private void AttachOperationsToEntity(TEntity entity)
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
			if (!operationIds.Any() || !_execution.curPrincipal.HasAnyOperations(_storage, operationIds))
				throw new UnreachableEntityTypeException<TEntity>();

			return entities;
		}

		protected virtual void CheckRightsForCreate(TEntity entity)
		{
			//TODO
			//var operations = new HashSet<int>(_appInstance.GetOperations<TEntity>(HttpVerb.POST).Select(o => o.Id));
			//if (!ExecutionContext.Current.curPrincipal.HasAnyOperations(_storage, _appInstance, operations))
			//{
			//	throw new HttpLikeException(HttpStatusCode.Unauthorized, String.Format("You cannot create entity of type {0}", typeof(TEntity).Name));
			//}
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
					el.Operations = operations.Where(op => entityPerms[el.Id].Contains(op.Id)).ToList();
				}
			}
		}

		protected virtual PatchEntityHelper GetPatcher(IStorageService storage)
		{
			return new PatchEntityHelper(storage);
		}

		/// <summary>
		/// Permet d'attacher des actions personnalisées en complément des opérations
		/// </summary>
		/// <param name="list"></param>
		internal virtual void AttachActions(IEnumerable<TEntity> list) { }
		private void AttachActionsToEntity(TEntity entity)
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

		public TEntity Create(object datas, Query<TEntity> query = null)
		{
			return Create(PostedData.ParseJSON(JsonConvert.SerializeObject(datas)), query);
		}
		public TEntity Create(PostedData datas, Query<TEntity> query = null)
		{
			var entity = InstanciateEntity();

			GetPatcher(_storage).PatchEntity(entity, datas);

			CheckRightsForCreate(entity);

			Create(entity, query);

			return entity;
		}
		public virtual void Create(TEntity entity, Query<TEntity> query = null)
		{
			if (query == null)
			{
				query = new Query<TEntity>();
			}

			//L'entité se complète elle même
			entity.Forge(_storage, query.Options);

			//On valide l'entité
			entity.Validate(_storage, null);

			Add(entity);
		}
		public virtual void CreateRange(IEnumerable<TEntity> entities, Query<TEntity> query = null)
		{
			if (query == null)
			{
				query = new Query<TEntity>();
			}

			foreach (var entity in entities)
			{
				//L'entité se complète elle même
				entity.Forge(_storage, query.Options);

				//On valide l'entité
				entity.Validate(_storage, null);
			}

			AddRange(entities);
		}
		public virtual TEntity InstanciateEntity()
		{
			return new TEntity();
		}

		private TEntity Update(TEntity entity, object datas, Query<TEntity> query = null)
		{
			return Update(entity, PostedData.ParseJSON(JsonConvert.SerializeObject(datas)), query);
		}
		public virtual TEntity Update(TEntity entity, PostedData datas, Query<TEntity> query = null)
		{
			if (query == null)
			{
				query = new Query<TEntity>();
			}

			AttachOperationsToEntity(entity);
			AttachActionsToEntity(entity);

			OnBeforeUpdateEntity(entity, datas);
			var oldEntity = entity.Clone();

			GetPatcher(_storage).PatchEntity(entity, datas);

			OnAfterUpdateEntity(oldEntity, entity, datas, query);

			entity.Validate(_storage, oldEntity);

			return entity;
		}
		public TEntity Update(TKey id, object datas, Query<TEntity> query = null)
		{
			return Update(id, PostedData.ParseJSON(JsonConvert.SerializeObject(datas)), query);
		}
		public TEntity Update(TKey id, PostedData datas, Query<TEntity> query = null)
		{
			var entity = GetById(id, HttpVerb.PUT);

			return Update(entity, datas, query);
		}
		//public ICollection<TEntity> Update(Query<TEntity> query, object datas)
		//{
		//	return Update(query, PostedData.ParseJSON(JsonConvert.SerializeObject(datas)));
		//}
		//public virtual ICollection<TEntity> Update(Query<TEntity> query, PostedData datas)
		//{
		//	var result = new HashSet<TEntity>();
		//	var entities = Get(query, CombinationVerb.PUT).Items;

		//	foreach(var entity in entities)
		//	{
		//		var item = Update(entity, datas);

		//		result.Add(item);
		//	}

		//	return entities;
		//}
		protected virtual void OnBeforeUpdateEntity(TEntity entity, PostedData datas) { }

		/// <summary>
		/// Called after entity update
		/// As "oldEntity" is a MemberWiseClone of "entity" before its update, it's a one level deep copy. If you want to go deeper
		/// you can do it by overriding the Clone() method and MemberWiseClone individual sub-properties
		/// </summary>
		/// <param name="oldEntity"></param>
		/// <param name="entity"></param>
		/// <param name="datas"></param>
		protected virtual void OnAfterUpdateEntity(TEntity oldEntity, TEntity entity, PostedData datas, Query<TEntity> query) { }

		internal protected virtual void Add(TEntity entity)
		{
			_storage.Add<TEntity>(entity);
		}
		internal protected virtual void AddRange(IEnumerable<TEntity> entities)
		{
			_storage.AddRange<TEntity>(entities);
		}

		public void Delete(TKey id)
		{
			var entity = GetById(id, HttpVerb.DELETE);

			AttachOperationsToEntity(entity);
			AttachActionsToEntity(entity);

			Delete(entity);
		}

		public virtual void Delete(TEntity entity)
		{
			Remove(entity);
		}
		public virtual void DeleteRange(IEnumerable<TEntity> entities)
		{
			RemoveRange(entities);
		}

		internal protected virtual void Remove(TEntity entity)
		{
			_storage.Remove<TEntity>(entity);
		}
		internal protected virtual void RemoveRange(IEnumerable<TEntity> entities)
		{
			_storage.RemoveRange<TEntity>(entities);
		}

		/// <summary>
		/// Get depuis du C# avec un predicat C#
		/// </summary>
		/// <param name="criteria"></param>
		/// <returns></returns>
		public IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> filter, HttpVerb verb = HttpVerb.GET)
		{
			return Get(new Query<TEntity> { ExpressionFilters = filter }, verb).Items;
		}
		public IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, object>> field, HttpVerb verb = HttpVerb.GET)
		{
			return Get(new Query<TEntity>(field, true) { ExpressionFilters = filter }, verb).Items;
		}

		/// <summary>
		/// Savoir s'il existent certaines ressources repondant a un filtre particulier
		/// </summary>
		/// <param name="filter"></param>
		/// <returns></returns>
		public bool Any(Expression<Func<TEntity, bool>> filter)
		{
			//Le Count() C# est plus rapide qu'un Any() SQL
			return Get(new Query<TEntity> { ExpressionFilters = filter, Options = new Options { NeedEnumeration = false, NeedCount = true } }, HttpVerb.GET).Count > 0;
		}

		public IEnumerable<TEntity> GetAll()
		{
			return Get(new Query<TEntity>(), HttpVerb.GET).Items;
		}

		public virtual ISelection<TEntity> Get(Query<TEntity> query, HttpVerb verb = HttpVerb.GET)
		{
			return Get(Set(query), query, verb);
		}

		protected virtual ISelection<TEntity> Get(IQueryable<TEntity> entities, Query<TEntity> query, HttpVerb verb = HttpVerb.GET)
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

				count = entities.Count();

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

				items = entities.ToList();

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

				items = Prepare(items, query);

				//ON attache les actions après le Prepare, histoire que les objets soient le plus complets possibles
				if (query.Options.attachActions)
				{
					AttachActionsToEntities(items);
				}
			}

			//Si c'était un PUT/DELETE, on en profite pour affiner la réponse
			if (verb != HttpVerb.GET && count == 0 && items.Count() == 0 && Any(i => true))
			{
				throw new UnauthorizedException(String.Format("Verb {0} unauthorized on entity type {1}", verb, typeof(TEntity).Name));
			}

			return new Selection<TEntity>(items, count);
		}

		public object TryGetById(object id, HttpVerb verb = HttpVerb.GET)
		{
			return GetById((TKey)id, verb);
		}
		public IEnumerable<object> TryGetByIds(IEnumerable<object> id, HttpVerb verb = HttpVerb.GET)
		{
			return GetByIds(new HashSet<TKey>(id.Cast<TKey>()), verb).Cast<object>();
		}

		public TEntity GetById(TKey id, HttpVerb verb = HttpVerb.GET)
		{
			return GetById(id, new Query<TEntity>() { Verb = verb }, verb);
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
		public virtual TEntity GetById(TKey id, Query<TEntity> query, HttpVerb verb = HttpVerb.GET)
		{
			var result = GetByIds(new HashSet<TKey> { id }, query, verb).FirstOrDefault();

			if (result == null)
			{
				//NB : si verb == PUT alors l'exception UnAuthorized sera levée lors du GetByIds
				throw new HttpLikeException(HttpStatusCode.NotFound, String.Format("Resource with ID {0} not found", id));
			}

			return result;
		}

		public IEnumerable<TEntity> GetByIds(ISet<TKey> ids, HttpVerb verb = HttpVerb.GET)
		{
			return GetByIds(ids, new Query<TEntity>(), verb);
		}
		public virtual IEnumerable<TEntity> GetByIds(ISet<TKey> ids, Query<TEntity> query, HttpVerb verb = HttpVerb.GET)
		{
			query.ExpressionFilters = Equals("id", ids.ToList()).Expand();
			return Get(query, verb).Items;
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
			includes.Remove(t => t.Operations);
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
		protected HashSet<int> GetOperationIds(Query<TEntity> query, HttpVerb verb)
		{
			IEnumerable<Operation> result = new List<Operation>();
			//On ne permet de filtrer sur certaines opérations qu'en GET
			//Sinon le user pourrait tenter un PUT avec l'opération "view" et ainsi modifier les entités qu'il peut voir !
			//Par contre il a le droit de voir les entités qu'il peut modifier, même si ce n'est pas paramétré comme ça dans ses rôles
			//if (query.Options.FilterOperations != null && verb == HttpVerb.GET)
			//{
			//	var filters = Where.ParseOperations<TEntity>(query.Options.FilterOperations);
			//	var predicate = new PredicateService(filters).GetPredicate<Operation>();

			//	result = _appInstance.GetAllOperations<TEntity>().AsQueryable().Where(predicate);
			//}
			//else
			//{
			//	result = _appInstance.GetOperations<TEntity>(verb);
			//}
			return new HashSet<int>(result.Select(o => o.Id));
		}

		public virtual IEnumerable<TEntity> Prepare(IEnumerable<TEntity> entities, Query<TEntity> query)
		{
			return entities;
		}
	}
}
