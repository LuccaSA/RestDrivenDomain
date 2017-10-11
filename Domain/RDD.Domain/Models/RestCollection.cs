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
            var postedData = PostedData.ParseJSON(JsonConvert.SerializeObject(datas));

            return CreateAsync(postedData, query);
		}
		public virtual Task<TEntity> CreateAsync(PostedData datas, Query<TEntity> query = null)
		{
			var entity = InstanciateEntity();

			GetPatcher().PatchEntity(entity, datas);

            return CreateAsync(entity);
		}
		public virtual Task<TEntity> CreateAsync(TEntity entity, Query<TEntity> query = null)
		{
            return CreateAsync(entity);
		}
        private async Task<TEntity> CreateAsync(TEntity entity)
        {
            await CheckRightsForCreateAsync(entity);

            ForgeEntity(entity);

            ValidateEntity(entity, null);

            _repository.Add(entity);

            return entity;
        }

        public virtual TEntity InstanciateEntity()
		{
			return new TEntity();
		}
		protected virtual void ForgeEntity(TEntity entity) { }
		protected virtual void ValidateEntity(TEntity entity, TEntity oldEntity) { }

		public Task<TEntity> UpdateByIdAsync(TKey id, object datas, Query<TEntity> query = null)
		{
            var postedData = PostedData.ParseJSON(JsonConvert.SerializeObject(datas));

            return UpdateByIdAsync(id, postedData, query);
		}
		public async virtual Task<TEntity> UpdateByIdAsync(TKey id, PostedData datas, Query<TEntity> query = null)
		{
			query = query ?? new Query<TEntity>();
            query.Verb = HttpVerb.PUT;
            query.Options.AttachActions = true;
            query.Options.AttachOperations = true;

            var entity = await GetByIdAsync(id, query);

            return await UpdateAsync(entity, datas, query);
		}

        public async virtual Task<IEnumerable<TEntity>> UpdateByIdsAsync(IDictionary<TKey, PostedData> datasByIds, Query<TEntity> query = null)
        {
            query = query ?? new Query<TEntity>();
            query.Verb = HttpVerb.PUT;
            query.Options.AttachActions = true;
            query.Options.AttachOperations = true;

            var ids = datasByIds.Keys.ToList();
            var entities = (await GetByIdsAsync(ids, query))
                .ToDictionary(el => el.Id, el => el);
            var result = new HashSet<TEntity>();

            foreach (var kvp in datasByIds)
            {
                var entity = entities[kvp.Key];
                entity = await UpdateAsync(entity, kvp.Value, query);

                result.Add(entity);
            }

            return result;
        }

        private async Task<TEntity> UpdateAsync(TEntity entity, PostedData datas, Query<TEntity> query)
        {
            await OnBeforeUpdateEntity(entity, datas);

            var oldEntity = entity.Clone();

            GetPatcher().PatchEntity(entity, datas);

            await OnAfterUpdateEntity(oldEntity, entity, datas, query);

            ValidateEntity(entity, oldEntity);

            return entity;
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

		public async virtual Task DeleteByIdAsync(TKey id)
		{
            var entity = await GetByIdAsync(id, new Query<TEntity> { Verb = HttpVerb.DELETE });

            _repository.Remove(entity);
		}

        public async virtual Task DeleteByIdsAsync(IList<TKey> ids)
        {
            var entities = await GetByIdsAsync(ids, new Query<TEntity> { Verb = HttpVerb.DELETE });

            foreach (var entity in entities)
            {
                _repository.Remove(entity);
            }
        }
    }
}
