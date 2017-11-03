using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RDD.Domain.Exceptions;
using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying;

namespace RDD.Domain.Models
{
    public class RestCollection<TEntity, TKey> : ReadOnlyRestCollection<TEntity, TKey>, IRestCollection<TEntity, TKey>
        where TEntity : class, IEntityBase<TEntity, TKey>, new()
        where TKey : IEquatable<TKey>
    {
        public RestCollection(IRepository<TEntity> repository, IExecutionContext execution, ICombinationsHolder combinationsHolder)
            : base(repository, execution, combinationsHolder)
        {
        }

        public Task<TEntity> CreateAsync(object datas, Query<TEntity> query = null)
        {
            PostedData postedData = PostedData.ParseJson(JsonConvert.SerializeObject(datas));

            return CreateAsync(postedData, query);
        }

        public virtual Task<TEntity> CreateAsync(PostedData datas, Query<TEntity> query = null)
        {
            TEntity entity = InstanciateEntity();

            GetPatcher().PatchEntity(entity, datas);

            return CreateAsync(entity);
        }

        public virtual async Task<TEntity> CreateAsync(TEntity entity, Query<TEntity> query)
        {
            await CheckRightsForCreateAsync(entity);

            ForgeEntity(entity);

            ValidateEntity(entity, null);

            Repository.Add(entity);

            return entity;
        }

        private Task<TEntity> CreateAsync(TEntity entity) => CreateAsync(entity, null); 

        public Task<TEntity> UpdateByIdAsync(TKey id, object datas, Query<TEntity> query = null)
        {
            PostedData postedData = PostedData.ParseJson(JsonConvert.SerializeObject(datas));

            return UpdateByIdAsync(id, postedData, query);
        }

        public virtual async Task<TEntity> UpdateByIdAsync(TKey id, PostedData datas, Query<TEntity> query = null)
        {
            query = query ?? new Query<TEntity>();
            query.Verb = HttpVerbs.Put;
            query.Options.AttachActions = true;
            query.Options.AttachOperations = true;

            TEntity entity = await GetByIdAsync(id, query);

            return await UpdateAsync(entity, datas, query);
        }

        public virtual async Task<IEnumerable<TEntity>> UpdateByIdsAsync(IDictionary<TKey, PostedData> datasByIds, Query<TEntity> query = null)
        {
            query = query ?? new Query<TEntity>();
            query.Verb = HttpVerbs.Put;
            query.Options.AttachActions = true;
            query.Options.AttachOperations = true;

            List<TKey> ids = datasByIds.Keys.ToList();
            Dictionary<TKey, TEntity> entities = (await GetByIdsAsync(ids, query)).ToDictionary(el => el.Id, el => el);

            var result = new HashSet<TEntity>();

            foreach (KeyValuePair<TKey, PostedData> kvp in datasByIds)
            {
                TEntity entity = entities[kvp.Key];
                entity = await UpdateAsync(entity, kvp.Value, query);

                result.Add(entity);
            }

            return result;
        }

        public virtual async Task DeleteByIdAsync(TKey id)
        {
            TEntity entity = await GetByIdAsync(id, new Query<TEntity>
            {
                Verb = HttpVerbs.Delete
            });

            Repository.Remove(entity);
        }

        public virtual async Task DeleteByIdsAsync(IList<TKey> ids)
        {
            IEnumerable<TEntity> entities = await GetByIdsAsync(ids, new Query<TEntity>
            {
                Verb = HttpVerbs.Delete
            });

            foreach (TEntity entity in entities)
            {
                Repository.Remove(entity);
            }
        }
        public virtual TEntity InstanciateEntity() => new TEntity();

        protected virtual Task CheckRightsForCreateAsync(TEntity entity)
        {
            IEnumerable<int> operationIds = CombinationsHolder.Combinations
                .Where(c => c.Subject == typeof(TEntity) && c.Verb == HttpVerbs.Post)
                .Select(c => c.Operation.Id);

            if (!Execution.curPrincipal.HasAnyOperations(new HashSet<int>(operationIds)))
            {
                throw new HttpLikeException(HttpStatusCode.Unauthorized, string.Format("You cannot create entity of type {0}", typeof(TEntity).Name));
            }

            return Task.CompletedTask;
        }

        protected virtual PatchEntityHelper GetPatcher() => new PatchEntityHelper();

        protected virtual void ForgeEntity(TEntity entity)
        {
        }

        protected virtual void ValidateEntity(TEntity entity, TEntity oldEntity)
        {
        }
        protected virtual Task OnBeforeUpdateEntity(TEntity entity, PostedData datas) => Task.CompletedTask;

        /// <summary>
        /// Called after entity update
        /// As "oldEntity" is a MemberWiseClone of "entity" before its update, it's a one level deep copy. If you want to go deeper
        /// you can do it by overriding the Clone() method and MemberWiseClone individual sub-properties
        /// </summary>
        protected virtual Task OnAfterUpdateEntity(TEntity oldEntity, TEntity entity, PostedData datas, Query<TEntity> query) => Task.CompletedTask;


        private async Task<TEntity> UpdateAsync(TEntity entity, PostedData datas, Query<TEntity> query)
        {
            await OnBeforeUpdateEntity(entity, datas);

            TEntity oldEntity = entity.Clone();

            GetPatcher().PatchEntity(entity, datas);

            await OnAfterUpdateEntity(oldEntity, entity, datas, query);

            ValidateEntity(entity, oldEntity);

            return entity;
        }
    }
}