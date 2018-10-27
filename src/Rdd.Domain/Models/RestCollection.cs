using Rdd.Domain.Helpers;
using Rdd.Domain.Models.Querying;
using Rdd.Domain.Patchers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rdd.Domain.Models
{
    public class RestCollection<TEntity, TKey> : ReadOnlyRestCollection<TEntity, TKey>, IRestCollection<TEntity, TKey>
        where TEntity : class, IEntityBase<TKey>
        where TKey : IEquatable<TKey>
    {
        protected new IRepository<TEntity, TKey> Repository { get; set; }
        protected IPatcher<TEntity> Patcher { get; set; }

        protected IInstanciator<TEntity> Instanciator { get; set; }

        public RestCollection(IRepository<TEntity, TKey> repository, IPatcher<TEntity> patcher, IInstanciator<TEntity> instanciator)
            : base(repository)
        {
            Patcher = patcher;
            Repository = repository;
            Instanciator = instanciator;
        }

        public virtual async Task<TEntity> CreateAsync(ICandidate<TEntity, TKey> candidate, IQuery<TEntity> query = null)
        {
            TEntity entity = Instanciator.InstanciateNew(candidate);

            entity = Patcher.Patch(entity, candidate.JsonValue);

            ForgeEntity(entity);

            if (await ValidateOrDiscardAsync(entity))
            {
                Repository.Add(entity);
                return entity;
            }

            return null;
        }

        public virtual async Task<IEnumerable<TEntity>> CreateAsync(IEnumerable<ICandidate<TEntity, TKey>> candidates, IQuery<TEntity> query = null)
        {
            var result = new List<TEntity>();

            foreach (var candidate in candidates)
            {
                TEntity entity = Instanciator.InstanciateNew(candidate);

                entity = Patcher.Patch(entity, candidate.JsonValue);

                ForgeEntity(entity);

                if (await ValidateOrDiscardAsync(entity))
                {
                    result.Add(entity);
                }
            }

            Repository.AddRange(result);

            return result;
        }

        public virtual async Task<TEntity> UpdateByIdAsync(TKey id, ICandidate<TEntity, TKey> candidate, IQuery<TEntity> query = null)
        {
            TEntity entity = await Repository.GetByIdAsync(id, query);
            if (entity == null)
            {
                return null;
            }
            return await UpdateAsync(entity, candidate, query);
        }

        public virtual async Task<IEnumerable<TEntity>> UpdateByIdsAsync(IDictionary<TKey, ICandidate<TEntity, TKey>> candidatesByIds, IQuery<TEntity> query = null)
        {
            var result = new HashSet<TEntity>();
            var ids = candidatesByIds.Select(d => d.Key).ToList();
            var entities = (await Repository.GetByIdsAsync(ids, query)).ToDictionary(el => el.Id);

            foreach (KeyValuePair<TKey, ICandidate<TEntity, TKey>> kvp in candidatesByIds)
            {
                TEntity entity = entities[kvp.Key];
                entity = await UpdateAsync(entity, kvp.Value, query);

                result.Add(entity);
            }

            return result;
        }

        public virtual async Task DeleteByIdAsync(TKey id, IQuery<TEntity> query)
        {
            var entity = await Repository.GetByIdAsync(id, query);
            if (entity != null)
            {
                Repository.Remove(entity);
            }
        }

        public virtual async Task DeleteByIdsAsync(IEnumerable<TKey> ids, IQuery<TEntity> query)
        {
            foreach (var entity in (await Repository.GetByIdsAsync(ids, query)))
            {
                Repository.Remove(entity);
            }
        }

        protected virtual void ForgeEntity(TEntity entity) { }

        private async Task<bool> ValidateOrDiscardAsync(TEntity entity)
        {
            bool isValid = false;
            try
            {
                isValid = await ValidateEntityAsync(entity);
            }
            finally
            {
                if (!isValid)
                {
                    Repository.DiscardChanges(entity);
                }
            }
            return isValid;
        }

        /// <summary>
        /// Validates an entity, and discard changes if entity is invalid.
        /// </summary>
        /// <param name="entity">The entity to validate</param>
        /// <returns>True if entity is valid</returns>
        protected virtual Task<bool> ValidateEntityAsync(TEntity entity) => Task.FromResult(true);

        protected virtual Task OnBeforeUpdateEntity(TEntity entity, ICandidate<TEntity, TKey> candidate) => Task.CompletedTask;

        /// <summary>
        /// Called after entity update
        /// As "oldEntity" is a MemberWiseClone of "entity" before its update, it's a one level deep copy. If you want to go deeper
        /// you can do it by overriding the Clone() method and MemberWiseClone individual sub-properties
        /// </summary>
        protected virtual Task OnAfterUpdateEntity(TEntity entity, ICandidate<TEntity, TKey> candidate, IQuery<TEntity> query) => Task.CompletedTask;

        private async Task<TEntity> UpdateAsync(TEntity entity, ICandidate<TEntity, TKey> candidate, IQuery<TEntity> query)
        {
            await OnBeforeUpdateEntity(entity, candidate);

            entity = Patcher.Patch(entity, candidate.JsonValue);

            await OnAfterUpdateEntity(entity, candidate, query);

            await ValidateOrDiscardAsync(entity);

            return entity;
        }
    }
}