using RDD.Domain.Contracts;
using RDD.Domain.Exceptions;
using RDD.Domain.Models.Convertors;
using RDD.Domain.Models.Querying;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace RDD.Domain.Models.Collections
{
	public partial class ReadOnlyRestCollection<TEntity, TKey> : IReadOnlyRestCollection<TEntity, TKey>
		where TEntity : class, IEntityBase<TKey>
		where TKey : IEquatable<TKey>
	{
		protected Stopwatch _queryWatch;

		IReadableRepository<TEntity> _repository;
		IQueryConvertor<TEntity> _convertor;

		protected ReadOnlyRestCollection() { }
		public ReadOnlyRestCollection(Stopwatch queryWatch, IReadableRepository<TEntity> repository, IQueryConvertor<TEntity> convertor)
		{
			_queryWatch = queryWatch;

			_repository = repository;
			_convertor = convertor;
		}

		protected void AttachOperationsToEntity(TEntity entity)
		{

		}
		private void AttachOperationsToEntities(IEnumerable<TEntity> entities)
		{
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
			AttachActions(list);
		}

		/// <summary>
		/// When a custom action needs to access the entities operations
		/// </summary>
		/// <param name="entities"></param>
		internal virtual void AppendOperationsToEntities(ICollection<TEntity> entities)
		{
		}
		
		public IEnumerable<TEntity> GetAll()
		{
			return Get(new Query<TEntity>()).Items;
		}

		public virtual ISelection<TEntity> Get(Query<TEntity> query)
		{
			var count = 0;
			IEnumerable<TEntity> items = new HashSet<TEntity>();

			var storageQuery = _convertor.Convert(query, _queryWatch);

			var needCount = query.Options.NeedCount && (!query.Options.NeedEnumeration || query.Options.withPagingInfo);
			if (needCount)
			{
				count = _repository.Count(storageQuery);
			}

			//En général on veut une énumération des entités
			if (query.Options.NeedEnumeration)
			{
				items = _repository.Get(storageQuery);

				//Ici on a énuméré, y'a pas eu de paging mais on veut le count, donc il n'a pas été fait en SQL, faut le faire en C#
				if (query.Options.NeedCount && !needCount)
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

			return new Selection<TEntity>(items, count);
		}

		public object TryGetById(object id)
		{
			try
			{
				return GetById((TKey)id);
			}
			catch { return null; }
		}

		public IEnumerable<object> TryGetByIds(IEnumerable<object> id)
		{
			try
			{
				return GetByIds(new HashSet<TKey>(id.Cast<TKey>())).Cast<object>();
			}
			catch { return null; }
		}

		public TEntity GetById(TKey id)
		{
			return GetById(id, new Query<TEntity>());
		}

		/// <summary>
		/// Si on ne trouve pas l'entité, on renvoie explicitement un NotFound
		/// puisque c'était explicitement cette entité qui était visée
		/// NB : on ne sait pas si l'entité existe mais qu'on n'y a pas accès ou si elle n'existe pas, mais c'est logique
		/// </summary>
		/// <param name="request"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		public virtual TEntity GetById(TKey id, Query<TEntity> query)
		{
			var result = GetByIds(new HashSet<TKey> { id }, query).FirstOrDefault();

			if (result == null)
			{
				//NB : si verb == PUT alors l'exception UnAuthorized sera levée lors du GetByIds
				throw new NotFoundException(String.Format("Resource with ID {0} not found", id));
			}

			return result;
		}

		public IEnumerable<TEntity> GetByIds(ISet<TKey> ids)
		{
			return GetByIds(ids, new Query<TEntity>());
		}
		public virtual IEnumerable<TEntity> GetByIds(ISet<TKey> ids, Query<TEntity> query)
		{
			query.Filters.Add(new Where { Field = "id", Type = WhereOperand.Equals, Values = ids.ToList() });
			return Get(query).Items;
		}

		public virtual IEnumerable<TEntity> Prepare(IEnumerable<TEntity> entities, Query<TEntity> query)
		{
			return entities;
		}
	}
}
