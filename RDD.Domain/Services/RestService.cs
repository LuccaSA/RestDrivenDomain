using LinqKit;
using Newtonsoft.Json;
using RDD.Infra.Helpers;
using RDD.Infra.Models.Enums;
using RDD.Infra.Models.Exceptions;
using RDD.Infra.Models.Querying;
using RDD.Infra.Models.Rights;
using RDD.Infra.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain.Services
{
	public partial class RestService<TEntity, IEntity, TKey> : IRestService<TEntity, IEntity, TKey>
		where TEntity : class, IEntity, new()
		where IEntity : IEntityBase<TKey>
		where TKey : IEquatable<TKey>
	{
		protected IAppInstance _appInstance;
		protected IStorageService _storage;
		protected IExecutionContext _execution;

		public RestService(IStorageService storage, IExecutionContext execution, string appTag = "")
		{
			_appInstance = GetAppInstanceByTag(appTag);
			_storage = storage;
			_execution = execution;
		}

		internal protected IQueryable<IEntity> Set()
		{
			return Set(new Query<IEntity>());
		}
		internal protected virtual IQueryable<IEntity> Set(Query<IEntity> query)
		{
			return _storage.Set<TEntity>();
		}
		protected virtual IQueryable<IEntity> FilterRights(IQueryable<IEntity> entities, Query<IEntity> query, HttpVerb verb)
		{
			if (verb != HttpVerb.GET)
			{
				throw new HttpLikeException(HttpStatusCode.Forbidden, String.Format("POST, PUT, DELETE forbidden by default on entity {0}", typeof(IEntity).Name));
			}

			return entities;
		}

		protected virtual void CheckRightsForCreate(IEntity entity)
		{
			throw new HttpLikeException(HttpStatusCode.Forbidden, String.Format("Creation forbidden by default on entity {0}", typeof(IEntity).Name));
		}

		protected virtual void AttachOperations(ICollection<IEntity> entities, List<Operation> operations)
		{
			var principal = _execution.curPrincipal;

			if (operations.Any())
			{
				var operationIds = operations.Select(op => op.Id);

				var result = _storage.Set<Permission>()
											.Where(p => principal.RolesID.Contains(p.RoleID)
												&& p.AppInstanceID == _appInstance.Id
												&& operationIds.Contains(p.OperationID))
											.Select(p => p.OperationID)
											.ToList();

				var entityPerms = entities.ToDictionary(o => o.Id, o => result);

				SetOperationsOnEntities(entities, entityPerms, operations);
			}
		}

		protected void SetOperationsOnEntities(ICollection<IEntity> list, Dictionary<TKey, List<int>> entityPerms, List<Operation> operations)
		{
			foreach(var el in list)
			{
				if (entityPerms.ContainsKey(el.Id))
				{
					el.Operations = operations.Where(op => entityPerms[el.Id].Contains(op.Id)).ToList();
				}
			}
		}

		//protected virtual PatchEntityHelper GetPatcher(IRepoProvider provider, IEntityContext context)
		//{
		//	return new PatchEntityHelper(provider, context);
		//}

		/// <summary>
		/// Permet d'attacher des actions personnalisées en complément des opérations
		/// </summary>
		/// <param name="list"></param>
		protected virtual void AttachActions(ICollection<IEntity> list) { }
		private void AttachActionsToEntity(IEntity entity)
		{
			AttachActionsToEntities(new HashSet<IEntity> { entity });
		}
		private void AttachActionsToEntities(ICollection<IEntity> list)
		{
			//ExecutionContext.Current.queryWatch.Start();

			//AttachActions(list);

			//ExecutionContext.Current.queryWatch.Stop();
		}
		public IEntity Create(object datas)
		{
			return Create(PostedData.ParseJSON(JsonConvert.SerializeObject(datas)));
		}
		public IEntity Create(PostedData datas)
		{
			var entity = InstanciateEntity();

//			GetPatcher(RepoProvider.Current, _context).PatchEntity(entity, datas);

			CheckRightsForCreate(entity);

			Create(entity);

			return entity;
		}
		public virtual void Create(TEntity entity)
        {
			//L'entité se complète elle même
			entity.Forge(_storage, _appInstance);

			//On valide l'entité
			entity.Validate(_storage);

			Add(entity);
		}
		public virtual void CreateRange(IEnumerable<TEntity> entities)
		{
			foreach (var entity in entities)
			{
				//L'entité se complète elle même
				entity.Forge(_storage, _appInstance);

				//On valide l'entité
				entity.Validate(_storage);
			}

			AddRange(entities);
		}
		protected virtual TEntity InstanciateEntity()
		{
			return new TEntity();
		}

		private IEntity Update(IEntity entity, object datas)
		{
			return Update(entity, PostedData.ParseJSON(JsonConvert.SerializeObject(datas)));
		}
		protected virtual IEntity Update(IEntity entity, PostedData datas)
		{
			//AttachOperationsToEntity(entity);
			//AttachActionsToEntity(entity);

			//OnBeforeUpdateEntity(entity, datas);

			//GetPatcher(RepoProvider.Current, _context).PatchEntity(entity, datas);

			//entity.Validate(_context);

			//OnAfterUpdateEntity(entity, datas);

			return entity;
		}
		public IEntity Update(TKey id, object datas)
		{
			return Update(id, PostedData.ParseJSON(JsonConvert.SerializeObject(datas)));
		}
		public IEntity Update(TKey id, PostedData datas)
		{
			var entity = GetById(id, HttpVerb.PUT);

			return Update(entity, datas);
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
		protected virtual void OnBeforeUpdateEntity(IEntity entity, PostedData datas) { }
		protected virtual void OnAfterUpdateEntity(IEntity entity, PostedData datas) { }

		internal protected virtual void Add(IEntity entity)
		{
			//_storage.Add<TEntity, TKey>(entity);
		}
		internal protected virtual void AddRange(IEnumerable<IEntity> entities)
		{
			//_storage.AddRange<TEntity, TKey>(entities);
		}

		public void Delete(TKey id)
		{
			//var entity = GetById(id, HttpVerb.DELETE);

			//AttachOperationsToEntity(entity);
			//AttachActionsToEntity(entity);

			//Delete(entity);
		}

		public virtual void Delete(TEntity entity)
		{
			Remove(entity);
		}
		public virtual void DeleteRange(IEnumerable<TEntity> entities)
		{
			RemoveRange(entities);
		}

		internal protected virtual void Remove(IEntity entity)
		{
//			_storage.Remove<TEntity, TKey>(entity);
		}
		internal protected virtual void RemoveRange(IEnumerable<IEntity> entities)
		{
	//		_storage.RemoveRange<TEntity, TKey>(entities);
		}

		/// <summary>
		/// Get depuis du C# avec un predicat C#
		/// </summary>
		/// <param name="criteria"></param>
		/// <returns></returns>
		public IEnumerable<IEntity> Get(Expression<Func<IEntity, bool>> filter, HttpVerb verb)
		{
			return Get(new Query<IEntity> { ExpressionFilters = filter }, verb).Items;
		}
		public IEnumerable<IEntity> Get(Expression<Func<IEntity, bool>> filter, Field fields, HttpVerb verb)
		{
			return Get(new Query<IEntity>(fields, true) { ExpressionFilters = filter }, verb).Items;
		}

		/// <summary>
		/// Savoir s'il existent certaines ressources repondant a un filtre particulier
		/// </summary>
		/// <param name="filter"></param>
		/// <returns></returns>
		//public bool Any(Expression<Func<IEntity, bool>> filter)
		//{
		//	// http://jell.lucca.fr/forum/default.asp?intMenu=2&id_theme=634&theme=634&id_sujet=12158#
		//	return Get(new Query<IEntity> { ExpressionFilters = filter, Options = new Options { NeedEnumeration = false, NeedCount = true } }, HttpVerb.GET).Count > 0;
		//}

		public List<IEntity> GetAll()
		{
			return Get(new Query<IEntity>(), HttpVerb.GET).Items.ToList();
		}
		public virtual RestCollection<IEntity, TKey> Get(Query<IEntity> query, HttpVerb verb)
		{
			var entities = Set(query);

			//On filtre les entités selon celles que l'on peut voir
			entities = FilterRights(entities, query, verb);

			//On joue les Wheres
			entities = ApplyFilters(entities, query);

			var collection = new RestCollection<IEntity, TKey>();

			//Dans de rares cas on veut seulement, ou en plus, le count des entités
			//On le fait en SQL si et seulement si y'a pas d'énumération (auquel cas on le fait en C#)
			//Ou qu'il y a énumération avec du paging (en cas de paging, le count doit compter TOUTES les entités et pas juste celles paginées
			if (query.Options.NeedCount && (!query.Options.NeedEnumeration || query.Options.withPaging))
			{
				_execution.queryWatch.Start();

				collection.Count = entities.Count();

				_execution.queryWatch.Stop();
			}

			//En général on veut une énumération des entités
			if (query.Options.NeedEnumeration)
			{
				//Les orderby
				entities = ApplyOrderBys(entities, query);

				//Paging => BUG dans EF sur le paging ?!!
				if (query.Options.withPaging)
				{
					entities = entities.Skip(query.Options.Page.Offset).Take(query.Options.Page.Limit);
				}

				entities = Includes(entities, query.Includes, verb, query.Fields);

				_execution.queryWatch.Start();

				var items = entities.ToList();

				_execution.queryWatch.Stop();

				//Ici on a énuméré, y'a pas eu de paging mais on veut le count, donc il n'a pas été fait en SQL, faut le faire en C#
				if (!query.Options.withPaging && query.Options.NeedCount) 
				{
					collection.Count = items.Count();
				}

				//Si on a demandé les permissions, on va les chercher après énumération
				if (query.Options.attachOperations)
				{
					//AttachOperationsToEntities(items);
				}

				items = Prepare(items, query);

				//ON attache les actions après le Prepare, histoire que les objets soient le plus complets possibles
				if (query.Options.attachActions)
				{
					AttachActionsToEntities(items);
				}

				collection.Items = items;
			}

			//Si c'était un PUT/DELETE, on en profite pour affiner la réponse
			//if (verb != HttpVerb.GET && collection.Count == 0 && Any(i => true))
			//{
			//	throw new UnauthorizedException(String.Format("Verb {0} unauthorized on entity type {1}", verb, typeof(IEntity).Name));
			//}

			return collection;
		}

		public object TryGetById(object id, HttpVerb verb = HttpVerb.GET)
		{
			return GetById((TKey)id, verb);
		}

		public IEntity GetById(TKey id, HttpVerb verb)
		{
			return GetById(id, new Query<IEntity>(), verb);
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
		public virtual IEntity GetById(TKey id, Query<IEntity> query, HttpVerb verb)
		{
			var result = GetByIds(new HashSet<TKey> { id }, query, verb).FirstOrDefault();

			if (result == null)
			{
				//NB : si verb == PUT alors l'exception UnAuthorized sera levée lors du GetByIds
				throw new HttpLikeException(HttpStatusCode.NotFound, String.Format("Resource with ID {0} not found", id));
			}

			return result;
		}

		public ICollection<IEntity> GetByIds(ISet<TKey> ids, HttpVerb verb)
		{
			return GetByIds(ids, new Query<IEntity>(), verb);
		}
		public virtual ICollection<IEntity> GetByIds(ISet<TKey> ids, Query<IEntity> query, HttpVerb verb)
		{
			// L'expression précédente e => e.Id.Equals(id) faisait planter le ExpressionManipulationService, notamment avec l'ancienneté
			// La raison étant qu'avec l'ancienne expression, le type du paramètre etait EntityBase<T> au lieu de T
			// Et même si T dérive de EntityBase<T>, l'expression ne reconnaissait pas qu'ils étaient compatibles
			var expression = PredicateBuilder.False<TEntity>();
			foreach (var id in ids)
			{
				expression = expression.Or(Equals("id", id).Expand()).Expand();
			}
			//query.ExpressionFilters = expression.Expand();

			return Get(query, verb).Items;
		}

		private IQueryable<IEntity> ApplyOrderBys(IQueryable<IEntity> entities, Query<IEntity> query)
		{
			//OrderBy => si y'a pas de orderby, il faut en mettre 1 par défaut, sinon le Skip plante
			//if (query.OrderBys.Count == 0)
			//{
			//	if (query.Options.withPaging)
			//	{
			//		entities = OrderByDefault(entities);
			//	}
			//}
			//else
			//{
			//	for (int i = 0, length = query.OrderBys.Count; i < length; i++)
			//	{
			//		entities = OrderBy(entities, query.OrderBys[i].Field, query.OrderBys[i].Direction, i == 0);
			//	}
			//}

			return entities;
		}
		private IQueryable<IEntity> ApplyFilters(IQueryable<IEntity> entities, Query<IEntity> query)
		{
			var expressionFilters = query.ExpressionFilters; //Filtres C#
			var queryFilters = query.Filters; //Filtre via la couche web

			//Si y'a des filtres
			if (expressionFilters != null || queryFilters != null)
			{
				//Expression<Func<IEntity, bool>> filters;

				//Soit via C#
				//if (expressionFilters != null)
				//{
				//	filters = expressionFilters;
				//}
				//else //Soit via l'URL
				//{
				//	var predicate = new PredicateBuilderHelper(queryFilters);
				//	filters = predicate.GetEntityPredicate<TEntity, TKey>(this);
				//}

				//On joue les wheres
				//entities = entities.Where(filters);
			}

			return entities;
		}
		public virtual ICollection<string> HandleIncludes(ICollection<string> includes, HttpVerb verb, Field fields)
		{
			//On n'inclut pas les propriétés qui ne viennent pas de la BDD
			var nonDatabaseProperty = new HashSet<string> { "Operations", "Culture", "Application" };

			return includes.Where(i => !nonDatabaseProperty.Contains(i)).ToList();
		}
		public IQueryable<IEntity> Includes(IQueryable<IEntity> entities)
		{
			return Includes(entities, new HashSet<string>(), HttpVerb.GET, null);
		}
		public IQueryable<IEntity> Includes(IQueryable<IEntity> entities, ICollection<string> includes, HttpVerb verb, Field fields)
		{
			//includes = HandleIncludes(includes, verb, fields);

			//return _storage.Includes<TEntity, TKey>(entities, includes);
			return entities;
		}
		protected virtual IAppInstance GetAppInstanceByTag(string appTag)
		{
			var appInstances = (IAppInstanceService)RestServiceProvider.Get<IAppInstance, int>(_storage, _execution);

			return appInstances.GetInstanceByTag<IEntity>(appTag);
		}
		protected virtual IAppInstance GetAppInstanceById(int appInstanceID)
		{
			var appInstances = (IAppInstanceService)RestServiceProvider.Get<IAppInstance, int>(_storage, _execution);

			return appInstances.GetInstanceById<IEntity>(appInstanceID);
		}
		protected List<Operation> GetOperations(Query<IEntity> query, HttpVerb verb)
		{
			var appInstances = (IAppInstanceService)RestServiceProvider.Get<IAppInstance, int>(_storage, _execution);

			//On ne permet de filtrer sur certaines opérations qu'en GET
			//Sinon le user pourrait tenter un PUT avec l'opération "view" et ainsi modifier les entités qu'il peut voir !
			//Par contre il a le droit de voir les entités qu'il peut modifier, même si ce n'est pas paramétré comme ça dans ses rôles
			if (query.Options.FilterOperations != null && verb == HttpVerb.GET)
			{
				var filters = Filter.ParseOperations<IEntity>(query.Options.FilterOperations);
				var predicate = new PredicateBuilderHelper(filters).GetPredicate<Operation>();

				var allOperations = appInstances.GetAllOperations<IEntity>(_appInstance);
				return allOperations.AsQueryable().Where(predicate).ToList();
			}

			return appInstances.GetOperations<IEntity>(_appInstance, verb); ;
		}

		protected virtual List<IEntity> Prepare(List<IEntity> entities, Query<IEntity> query)
		{
			return entities;
		}
	}
}
