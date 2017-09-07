using Newtonsoft.Json;
using RDD.Domain.Contracts;
using RDD.Domain.Helpers;
using RDD.Domain.Models.Convertors;
using RDD.Domain.Models.Querying;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RDD.Domain.Models.Collections
{
	public partial class RestCollection<TEntity, TKey> : ReadOnlyRestCollection<TEntity, TKey>, IRestCollection<TEntity,TKey>
		where TEntity : class, IEntityBase<TKey>, new()
		where TKey : IEquatable<TKey>
	{
		protected IRepository<TEntity> _repository;

		public RestCollection(Stopwatch queryWatch, IRepository<TEntity> repository, IQueryConvertor<TEntity> convertor)
			: base(queryWatch, repository, convertor)
		{
			_repository = repository;
		}

		protected virtual PatchEntityHelper GetPatcher()
		{
			return new PatchEntityHelper();
		}
		
		public TEntity Create(PostedData datas, Query<TEntity> query = null)
		{
			var entity = InstanciateEntity();

			GetPatcher().PatchEntity(entity, datas);
			
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
			
			_repository.Add(entity);
		}
		public virtual TEntity GetEntityAfterCreate(TEntity entity, Query<TEntity> query = null)
		{
			return GetById(entity.Id, query);
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
			}
			
			_repository.AddRange(entities);
		}
		public virtual TEntity InstanciateEntity()
		{
			return new TEntity();
		}
		protected virtual void ForgeEntity(TEntity entity, Options queryOptions) { }

		public TEntity Update(TKey id, object datas, Query<TEntity> query = null)
		{
			return Update(id, PostedData.ParseJSON(JsonConvert.SerializeObject(datas)), query);
		}
		public TEntity Update(TKey id, PostedData datas, Query<TEntity> query = null)
		{
			return Update(GetById(id), datas, query);
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

			GetPatcher().PatchEntity(entity, datas);

			_repository.Update(entity);

			return entity;
		}

		protected virtual void OnBeforeUpdateEntity(TEntity entity, PostedData datas) { }

		/// <summary>
		/// Called after entity update
		/// As "oldEntity" is a MemberWiseClone of "entity" before its update, it's a one level deep copy. If you want to go deeper
		/// you can do it by overriding the Clone() method and MemberWiseClone individual sub-properties
		/// </summary>
		/// <param name="oldEntity"></param>
		/// <param name="entity"></param>
		/// <param name="datas"></param>
		protected virtual void OnAfterUpdateEntity(TEntity entity, PostedData datas, Query<TEntity> query) { }
		
		public void Delete(TKey id)
		{
			var entity = GetById(id);

			AttachOperationsToEntity(entity);
			AttachActionsToEntity(entity);

			Delete(entity);
		}

		public virtual void Delete(TEntity entity)
		{
			_repository.Delete(entity);
		}
		public virtual void DeleteRange(IEnumerable<TEntity> entities)
		{
			_repository.DeleteRange(entities);
		}
	}
}
