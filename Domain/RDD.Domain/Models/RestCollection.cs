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
	public class RestCollection<TEntity, TKey> : ReadOnlyRestCollection<TEntity, TKey>, IRestCollection<TEntity, TKey>
		where TEntity : class, IEntityBase<TEntity, TKey>, new()
		where TKey : IEquatable<TKey>
	{
		public RestCollection(IRepository<TEntity> repository, IExecutionContext execution, ICombinationsHolder combinationsHolder)
			: base(repository, execution, combinationsHolder) { }

		protected virtual Task CheckRightsForCreateAsync(TEntity entity)
		{
			var operationIds = _combinationsHolder.Combinations
				.Where(c => c.Subject == typeof(TEntity) && c.Verb == HttpVerb.POST)
				.Select(c => c.Operation.Id);

			if (!_execution.curPrincipal.HasAnyOperations(new HashSet<int>(operationIds)))
			{
				throw new HttpLikeException(HttpStatusCode.Unauthorized, String.Format("You cannot create entity of type {0}", typeof(TEntity).Name));
			}

			return Task.CompletedTask;
		}

		protected virtual PatchEntityHelper GetPatcher()
		{
			return new PatchEntityHelper();
		}

		public Task<TEntity> CreateAsync(object datas, Query<TEntity> query = null)
		{
			return CreateAsync(PostedData.ParseJSON(JsonConvert.SerializeObject(datas)), query);
		}
		public async Task<TEntity> CreateAsync(PostedData datas, Query<TEntity> query = null)
		{
			var entity = InstanciateEntity();

			GetPatcher().PatchEntity(entity, datas);

			await CheckRightsForCreateAsync(entity);

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
			ValidateEntity(entity, null);

			_repository.Add(entity);
		}
		public virtual Task<TEntity> GetEntityAfterCreateAsync(TEntity entity, Query<TEntity> query = null)
		{
			return GetByIdAsync(entity.Id, query);
		}

		public virtual TEntity InstanciateEntity()
		{
			return new TEntity();
		}
		protected virtual void ForgeEntity(TEntity entity, Options queryOptions) { }
		protected virtual void ValidateEntity(TEntity entity, TEntity oldEntity) { }

		private Task<TEntity> UpdateAsync(TEntity entity, object datas, Query<TEntity> query = null)
		{
			return UpdateAsync(entity, PostedData.ParseJSON(JsonConvert.SerializeObject(datas)), query);
		}
		public async virtual Task<TEntity> UpdateAsync(TEntity entity, PostedData datas, Query<TEntity> query = null)
		{
			if (query == null)
			{
				query = new Query<TEntity>();
			}

			AttachOperationsToEntity(entity);
			AttachActionsToEntity(entity);

			await OnBeforeUpdateEntity(entity, datas);
			var oldEntity = entity.Clone();

			GetPatcher().PatchEntity(entity, datas);

			await OnAfterUpdateEntity(oldEntity, entity, datas, query);

			ValidateEntity(entity, oldEntity);

			return entity;
		}
		public Task<TEntity> UpdateAsync(TKey id, object datas, Query<TEntity> query = null)
		{
			return UpdateAsync(id, PostedData.ParseJSON(JsonConvert.SerializeObject(datas)), query);
		}
		public async Task<TEntity> UpdateAsync(TKey id, PostedData datas, Query<TEntity> query = null)
		{
			var entity = await GetByIdAsync(id, HttpVerb.PUT);

			return await UpdateAsync(entity, datas, query);
		}

		protected virtual Task OnBeforeUpdateEntity(TEntity entity, PostedData datas)
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Called after entity update
		/// As "oldEntity" is a MemberWiseClone of "entity" before its update, it's a one level deep copy. If you want to go deeper
		/// you can do it by overriding the Clone() method and MemberWiseClone individual sub-properties
		/// </summary>
		/// <param name="oldEntity"></param>
		/// <param name="entity"></param>
		/// <param name="datas"></param>
		protected virtual Task OnAfterUpdateEntity(TEntity oldEntity, TEntity entity, PostedData datas, Query<TEntity> query)
		{
			return Task.CompletedTask;
		}

		public async Task DeleteAsync(TKey id)
		{
			var entity = await GetByIdAsync(id, HttpVerb.DELETE);

			AttachOperationsToEntity(entity);
			AttachActionsToEntity(entity);

			Delete(entity);
		}

		public virtual void Delete(TEntity entity)
		{
			_repository.Remove(entity);
		}
	}
}
