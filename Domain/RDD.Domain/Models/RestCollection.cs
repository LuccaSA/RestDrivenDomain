using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying;
using RDD.Domain.Patchers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RDD.Domain.Models
{
    public class RestCollection<TEntity, TKey> : ReadOnlyRestCollection<TEntity, TKey>, IRestCollection<TEntity, TKey>
        where TEntity : class, IEntityBase<TEntity, TKey>
        where TKey : IEquatable<TKey>
    {
        protected new IRepository<TEntity> Repository { get; set; }
        protected IPatcher<TEntity> Patcher { get; set; }

        protected IInstanciator<TEntity> Instanciator { get;set;}

        public RestCollection(IRepository<TEntity> repository, IPatcher<TEntity> patcher, IInstanciator<TEntity> instanciator)
            : base(repository)
        {
            Patcher = patcher;
            Repository = repository;
            Instanciator = instanciator;
        }

        public virtual async Task<TEntity> CreateAsync(ICandidate<TEntity, TKey> candidate, Query<TEntity> query = null)
        {
            TEntity entity = Instanciator.InstanciateNew(candidate);

            Patcher.Patch(entity, candidate.JsonValue);

            ForgeEntity(entity);

            ValidateEntity(entity, null);

            Repository.Add(entity);

            return entity;
        }

        public virtual async Task<IEnumerable<TEntity>> CreateAsync(IEnumerable<ICandidate<TEntity, TKey>> candidates, Query<TEntity> query = null)
        {
            var result = new List<TEntity>();

            foreach (var candidate in candidates)
            {
                TEntity entity = Instanciator.InstanciateNew(candidate);

                Patcher.Patch(entity, candidate.JsonValue);

                ForgeEntity(entity);

                ValidateEntity(entity, null);

                result.Add(entity);
            }

            Repository.AddRange(result);
            return result;
        }

        /// <summary>
        /// Called before the entity is created
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <returns>false if the entity should not be created</returns>
        protected virtual bool OnBeforeCreate(TEntity entity) => true;

        public virtual async Task<TEntity> UpdateByIdAsync(TKey id, ICandidate<TEntity, TKey> candidate, Query<TEntity> query = null)
        {
            query = query ?? new Query<TEntity>();
            query.Verb = HttpVerbs.Put;
            TEntity entity = await GetByIdAsync(id, query);
            if (entity == null)
            {
                return null;
            }
            return await UpdateAsync(entity, candidate, query);
        }

        public virtual async Task<TEntity> UpdateByIdAsync(TKey id, TEntity entity)
        {
            TEntity oldEntity = await GetByIdAsync(id, new Query<TEntity> { Verb = HttpVerbs.Put });
            if (oldEntity == null)
            {
                return null;
            }

            UpdateEntityCore(id, entity, oldEntity);

            return entity;
        }

        public virtual async Task<IEnumerable<TEntity>> UpdateByIdsAsync(IDictionary<TKey, ICandidate<TEntity, TKey>> candidatesByIds, Query<TEntity> query = null)
        {
            query = query ?? new Query<TEntity>();
            query.Verb = HttpVerbs.Put;

            var result = new HashSet<TEntity>();

            var ids = candidatesByIds.Select(d => d.Key).ToList();
            var expQuery = new Query<TEntity>(query, e => ids.Contains(e.Id));
            var entities = (await GetAsync(expQuery)).Items.ToDictionary(el => el.Id);

            foreach (KeyValuePair<TKey, ICandidate<TEntity, TKey>> kvp in candidatesByIds)
            {
                TEntity entity = entities[kvp.Key];
                entity = await UpdateAsync(entity, kvp.Value, query);

                result.Add(entity);
            }

            return result;
        }

        public virtual async Task DeleteByIdAsync(TKey id)
        {
            var entity = await GetByIdAsync(id, new Query<TEntity> { Verb = HttpVerbs.Delete });
            if (entity != null)
            {
                Repository.Remove(entity);
            }
        }

        public virtual async Task DeleteByIdsAsync(IEnumerable<TKey> ids)
        {
            var expQuery = new Query<TEntity>(e => ids.Contains(e.Id)) { Verb = HttpVerbs.Delete };
            
            foreach (var entity in (await GetAsync(expQuery)).Items)
            {
                Repository.Remove(entity);
            }
        }

        protected virtual void ForgeEntity(TEntity entity) { }

        protected virtual bool ValidateEntity(TEntity entity, TEntity oldEntity) => true;

        protected virtual Task OnBeforePatchEntity(TEntity entity, ICandidate<TEntity, TKey> candidate) => Task.CompletedTask;
        
        /// <summary>
        /// Called after entity update
        /// As "oldEntity" is a MemberWiseClone of "entity" before its update, it's a one level deep copy. If you want to go deeper
        /// you can do it by overriding the Clone() method and MemberWiseClone individual sub-properties
        /// </summary>
        protected virtual Task OnAfterPatchEntity(TEntity oldEntity, TEntity entity, ICandidate<TEntity, TKey> candidate, Query<TEntity> query) => Task.CompletedTask;

        private async Task<TEntity> UpdateAsync(TEntity oldEntity, ICandidate<TEntity, TKey> candidate, Query<TEntity> query)
        {
            await OnBeforePatchEntity(oldEntity, candidate);

            TEntity newEntity = oldEntity.Clone();

            GetPatcher().Patch(oldEntity, candidate.JsonValue);

            await OnAfterPatchEntity(oldEntity, newEntity, candidate, query);

            Patcher.Patch(entity, candidate.JsonValue);
            UpdateEntityCore((TKey)newEntity.GetId(), newEntity, oldEntity);

            return oldEntity;
        }

        private void UpdateEntityCore(TKey id, TEntity entity, TEntity oldEntity)
        {
            if (!ValidateEntity(entity, oldEntity))
            {
                return;
            }

            Repository.Update<TEntity, TKey>(id, entity);
        }
    }
}