using LinqKit;
using Newtonsoft.Json;
using RDD.Infra.Helpers;
using RDD.Domain.Helpers;
using RDD.Infra;
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
	public abstract partial class RestDomainService<TEntity, TKey> : IRestDomainService<TEntity, TKey>
		where TEntity : class, IEntityBase<TKey>
		where TKey : IEquatable<TKey>
	{
		protected IAppInstance _appInstance;
		protected IStorageService _storage;
		protected IExecutionContext _execution;

		public RestDomainService(IStorageService storage, IExecutionContext execution, string appTag = "")
		{
			_storage = storage;
			_execution = execution;
			_appInstance = GetAppInstanceByTag(appTag);
		}

		internal protected IQueryable<TEntity> Set()
		{
			return Set(new Query<TEntity>());
		}
		internal protected virtual IQueryable<TEntity> Set(Query<TEntity> query)
		{
			return _storage.Set<TEntity>();
		}
		public TEntity GetById(TKey id, HttpVerb verb = HttpVerb.GET)
		{
			return GetById(id, new Query<TEntity>(), verb);

		}
		public TEntity GetById(TKey id, Query<TEntity> query, HttpVerb verb = HttpVerb.GET)
		{
			var result = GetByIds(new HashSet<TKey> { id }, query, verb).FirstOrDefault();

			if (result == null)
			{
				//NB : si verb == PUT alors l'exception UnAuthorized sera levée lors du GetByIds
				throw new HttpLikeException(HttpStatusCode.NotFound, String.Format("Resource with ID {0} not found", id));
			}

			return result;
		}
		public ICollection<TEntity> GetByIds(ISet<TKey> ids, HttpVerb verb)
		{
			return GetByIds(ids, new Query<TEntity>(), verb);
		}
		public ICollection<TEntity> GetByIds(ISet<TKey> ids, Query<TEntity> query, HttpVerb verb = HttpVerb.GET)
		{
			// L'expression précédente e => e.Id.Equals(id) faisait planter le ExpressionManipulationService, notamment avec l'ancienneté
			// La raison étant qu'avec l'ancienne expression, le type du paramètre etait EntityBase<T> au lieu de T
			// Et même si T dérive de EntityBase<T>, l'expression ne reconnaissait pas qu'ils étaient compatibles
			var expression = PredicateBuilder.False<TEntity>();
			foreach (var id in ids)
			{
				var equalsExpression = ExpressionManipulationHelper.ApplyExpressionToParameterType<TEntity, TEntity>(Equals("id", id));
				expression = expression.Or(equalsExpression.Expand()).Expand();
			}
			query.ExpressionFilters = expression.Expand();

			return Get(query, verb).Items;
		}
		public List<TEntity> GetAll()
		{
			return Get(new Query<TEntity>(), HttpVerb.GET).Items.ToList();
		}
		public IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> filter, HttpVerb verb)
		{
			return Get(new Query<TEntity> { ExpressionFilters = filter }, verb).Items;
		}
		public RestCollection<TEntity, TKey> Get(Query<TEntity> query, HttpVerb verb = HttpVerb.GET)
		{
			var entities = Set(query);

			//On filtre les entités selon celles que l'on peut voir
			entities = FilterRights(entities, query, verb);

			//On joue les Wheres
			entities = ApplyFilters(entities, query);

			var collection = new RestCollection<TEntity, TKey>();

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
			if (verb != HttpVerb.GET && collection.Count == 0 && Any(i => true))
			{
				throw new UnauthorizedException(String.Format("Verb {0} unauthorized on entity type {1}", verb, typeof(TEntity).Name));
			}

			return collection;
		}

		protected virtual IQueryable<TEntity> FilterRights(IQueryable<TEntity> entities, Query<TEntity> query, HttpVerb verb)
		{
			if (verb != HttpVerb.GET)
			{
				throw new HttpLikeException(HttpStatusCode.Forbidden, String.Format("POST, PUT, DELETE forbidden by default on entity {0}", typeof(TEntity).Name));
			}

			return entities;
		}

		protected virtual void CheckRightsForCreate(TEntity entity)
		{
			throw new HttpLikeException(HttpStatusCode.Forbidden, String.Format("Creation forbidden by default on entity {0}", typeof(TEntity).Name));
		}

		protected void SetOperationsOnEntities(ICollection<TEntity> list, Dictionary<TKey, List<int>> entityPerms, List<Operation> operations)
		{
			foreach(var el in list)
			{
				if (entityPerms.ContainsKey(el.Id))
				{
					el.Operations = operations.Where(op => entityPerms[el.Id].Contains(op.Id)).ToList();
				}
			}
		}

		private void AttachOperationsToEntity(TEntity entity)
		{
			AttachOperationsToEntities(new List<TEntity> { entity });
		}
		private void AttachOperationsToEntities(List<TEntity> entities)
		{
			var operationsForAttach = ((IAppInstancesService)RestServiceProvider.Get<IAppInstance, int>(_storage, _execution)).GetAllOperations<TEntity>(_appInstance);

			_execution.queryWatch.Start();

			AttachOperations(entities, operationsForAttach);

			_execution.queryWatch.Stop();
		}
		protected virtual void AttachOperations(ICollection<TEntity> entities, List<Operation> operations)
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

		protected virtual void AttachActions(ICollection<TEntity> list) { }
		private void AttachActionsToEntity(TEntity entity)
		{
			AttachActionsToEntities(new HashSet<TEntity> { entity });
		}
		private void AttachActionsToEntities(ICollection<TEntity> list)
		{
			_execution.queryWatch.Start();

			AttachActions(list);

			_execution.queryWatch.Stop();
		}
		protected abstract TEntity InstanciateEntity();
		public TEntity Create(object datas)
		{
			return Create(PostedData.ParseJSON(JsonConvert.SerializeObject(datas)));
		}
		public TEntity Create(PostedData datas)
		{
			var entity = InstanciateEntity();

			GetPatcher().PatchEntity(entity, datas);

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

		private TEntity Update(TEntity entity, object datas)
		{
			return Update(entity, PostedData.ParseJSON(JsonConvert.SerializeObject(datas)));
		}
		protected virtual TEntity Update(TEntity entity, PostedData datas)
		{
			AttachOperationsToEntity(entity);
			AttachActionsToEntity(entity);

			OnBeforeUpdateEntity(entity, datas);

			GetPatcher().PatchEntity(entity, datas);

			entity.Validate(_storage);

			OnAfterUpdateEntity(entity, datas);

			return entity;
		}
		public TEntity Update(TKey id, object datas)
		{
			return Update(id, PostedData.ParseJSON(JsonConvert.SerializeObject(datas)));
		}
		public TEntity Update(TKey id, PostedData datas)
		{
			var entity = GetById(id, HttpVerb.PUT);

			return Update(entity, datas);
		}
		public ICollection<TEntity> Update(Query<TEntity> query, object datas)
		{
			return Update(query, PostedData.ParseJSON(JsonConvert.SerializeObject(datas)));
		}
		public virtual ICollection<TEntity> Update(Query<TEntity> query, PostedData datas)
		{
			var result = new HashSet<TEntity>();
			var entities = Get(query, HttpVerb.PUT).Items;

			foreach (var entity in entities)
			{
				var item = Update(entity, datas);

				result.Add(item);
			}

			return entities;
		}
		protected virtual void OnBeforeUpdateEntity(TEntity entity, PostedData datas) { }
		protected virtual void OnAfterUpdateEntity(TEntity entity, PostedData datas) { }

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
			_storage.Remove<TEntity>(entity);
		}
		public virtual void DeleteRange(IEnumerable<TEntity> entities)
		{
			_storage.RemoveRange<TEntity>(entities);
		}

		/// <summary>
		/// Savoir s'il existent certaines ressources repondant a un filtre particulier
		/// </summary>
		/// <param name="filter"></param>
		/// <returns></returns>
		public bool Any(Expression<Func<TEntity, bool>> filter)
		{
			// http://jell.lucca.fr/forum/default.asp?intMenu=2&id_theme=634&theme=634&id_sujet=12158#
			return Get(new Query<TEntity> { ExpressionFilters = filter, Options = new Options { NeedEnumeration = false, NeedCount = true } }, HttpVerb.GET).Count > 0;
		}

		public object TryGetById(object id, HttpVerb verb = HttpVerb.GET)
		{
			return GetById((TKey)id, verb);
		}

		private IQueryable<TEntity> ApplyOrderBys(IQueryable<TEntity> entities, Query<TEntity> query)
		{
			//OrderBy => si y'a pas de orderby, il faut en mettre 1 par défaut, sinon le Skip plante
			if (query.OrderBys.Count == 0)
			{
				if (query.Options.withPaging)
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
		private IQueryable<TEntity> ApplyFilters(IQueryable<TEntity> entities, Query<TEntity> query)
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
					filters = ExpressionManipulationHelper.ApplyExpressionToParameterType<TEntity, TEntity>(expressionFilters);
				}
				else //Soit via l'URL
				{
					var predicate = new PredicateBuilderHelper(queryFilters);
					filters = predicate.GetEntityPredicate<TEntity, TKey>(this);
				}

				//On joue les wheres
				entities = entities.Where(filters);
			}

			return entities;
		}
		public virtual ICollection<string> HandleIncludes(ICollection<string> includes, HttpVerb verb, Field fields)
		{
			//On n'inclut pas les propriétés qui ne viennent pas de la BDD
			var nonDatabaseProperty = new HashSet<string> { "Operations", "Culture", "Application" };

			return includes.Where(i => !nonDatabaseProperty.Contains(i)).ToList();
		}
		public IQueryable<TEntity> Includes(IQueryable<TEntity> entities)
		{
			return Includes(entities, new HashSet<string>(), HttpVerb.GET, null);
		}
		public IQueryable<TEntity> Includes(IQueryable<TEntity> entities, ICollection<string> includes, HttpVerb verb, Field fields)
		{
			includes = HandleIncludes(includes, verb, fields);

			return _storage.Includes<TEntity>(entities, includes);
		}
		protected virtual PatchEntityHelper GetPatcher()
		{
			return new PatchEntityHelper(_storage, _execution);
		}
		protected abstract IAppInstance GetAppInstanceByTag(string appTag);
		protected abstract IAppInstance GetAppInstanceById(int appInstanceID);
		protected abstract List<Operation> GetOperations(Query<TEntity> query, HttpVerb verb);

		protected virtual List<TEntity> Prepare(List<TEntity> entities, Query<TEntity> query)
		{
			return entities;
		}
	}
}
