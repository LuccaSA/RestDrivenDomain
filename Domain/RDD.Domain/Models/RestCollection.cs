using Newtonsoft.Json;
using RDD.Domain.Exceptions;
using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RDD.Domain.Models
{
	public partial class RestCollection<TEntity, TKey> : ReadOnlyRestCollection<TEntity, TKey>, IRestCollection<TEntity, TKey>
		where TEntity : class, IEntityBase<TEntity, TKey>, new()
		where TKey : IEquatable<TKey>
	{
		public RestCollection(IStorageService storage, IExecutionContext execution, ICombinationsHolder combinationsHolder, Func<IStorageService> asyncStorage = null)
			: base(storage, execution, combinationsHolder, asyncStorage) { }

		protected virtual void CheckRightsForCreate(TEntity entity)
		{
			var operationIds = _combinationsHolder.Combinations
				.Where(c => c.Subject == typeof(TEntity) && c.Verb == HttpVerb.POST)
				.Select(c => c.Operation.Id);

			if (!_execution.curPrincipal.HasAnyOperations(_storage, new HashSet<int>(operationIds)))
			{
				throw new HttpLikeException(HttpStatusCode.Unauthorized, String.Format("You cannot create entity of type {0}", typeof(TEntity).Name));
			}
		}

		protected virtual PatchEntityHelper GetPatcher(IStorageService storage)
		{
			return new PatchEntityHelper(storage);
		}

		public async Task<TEntity> CreateAsync(object datas, Query<TEntity> query = null)
		{
			return await CreateAsync(PostedData.ParseJSON(JsonConvert.SerializeObject(datas)), query);
		}
		public async Task<TEntity> CreateAsync(PostedData datas, Query<TEntity> query = null)
		{
			var entity = InstanciateEntity();

			GetPatcher(_storage).PatchEntity(entity, datas);

			CheckRightsForCreate(entity);

			await CreateAsync(entity, query);

			return entity;
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

			ForgeEntity(entity, query.Options);

			//On valide l'entité
			entity.Validate(_storage, null);

			Add(entity);
		}
		public async virtual Task CreateAsync(TEntity entity, Query<TEntity> query = null)
		{
			if (query == null)
			{
				query = new Query<TEntity>();
			}

			ForgeEntity(entity, query.Options);

			//On valide l'entité
			entity.Validate(_storage, null);

			await AddAsync(entity);
		}
		public virtual TEntity GetEntityAfterCreate(TEntity entity, Query<TEntity> query = null)
		{
			return GetById(entity.Id, query, query.Verb);
		}
		public async virtual Task<TEntity> GetEntityAfterCreateAsync(TEntity entity, Query<TEntity> query = null)
		{
			return await GetByIdAsync(entity.Id, query, query.Verb);
		}

		public virtual void CreateRange(IEnumerable<TEntity> entities, Query<TEntity> query = null)
		{
			if (query == null)
			{
				query = new Query<TEntity>();
			}

			foreach (var entity in entities)
			{
				ForgeEntity(entity, query.Options);

				//On valide l'entité
				entity.Validate(_storage, null);
			}

			AddRange(entities);
		}
		public virtual TEntity InstanciateEntity()
		{
			return new TEntity();
		}
		protected virtual void ForgeEntity(TEntity entity, Options queryOptions) { }

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
		internal async protected virtual Task AddAsync(TEntity entity)
		{
			await _storage.AddAsync<TEntity>(entity);
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
	}
}
