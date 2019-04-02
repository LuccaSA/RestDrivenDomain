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
        protected new IRepository<TEntity> Repository { get; set; }
        protected IPatcher<TEntity> Patcher { get; set; }

        protected IInstanciator<TEntity> Instanciator { get; set; }

        public RestCollection(IRepository<TEntity> repository, IPatcher<TEntity> patcher, IInstanciator<TEntity> instanciator)
            : base(repository)
        {
            Patcher = patcher;
            Repository = repository;
            Instanciator = instanciator;
        }

        public async Task<TEntity> CreateAsync(ICandidate<TEntity, TKey> candidate)
        {
            var result = await (CreateAsync(candidate.Yield()));
            return result.First();
        }

        public Task<IEnumerable<TEntity>> CreateAsync(IEnumerable<ICandidate<TEntity, TKey>> candidates)
        {
            return CreateAsync(Patch(candidates));
        }

        public virtual async Task<IEnumerable<TEntity>> CreateAsync(IEnumerable<TEntity> entities)
        {
            var result = new List<TEntity>();

            foreach (var entity in entities)
            {
                ForgeEntity(entity);

                if (await ValidateOrDiscardAsync(entity))
                {
                    result.Add(entity);
                }
            }

            Repository.AddRange(result);

            return result;
        }

        public async Task<TEntity> UpdateByIdAsync(TKey id, ICandidate<TEntity, TKey> candidate, Query<TEntity> query = null)
        {
            var result = await UpdateByIdsAsync(new Dictionary<TKey, ICandidate<TEntity, TKey>> { { id, candidate } });
            return result.First();
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

        public Task DeleteByIdAsync(TKey id)
        {
            return DeleteByIdsAsync(id.Yield());
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
        protected virtual Task OnAfterUpdateEntity(TEntity entity, ICandidate<TEntity, TKey> candidate, Query<TEntity> query) => Task.CompletedTask;

        protected IEnumerable<TEntity> Patch(IEnumerable<ICandidate<TEntity, TKey>> candidates)
        {
            var result = new HashSet<TEntity>();
            foreach (var candidate in candidates)
            {
                TEntity entity = Instanciator.InstanciateNew(candidate);
                result.Add(Patcher.Patch(entity, candidate.JsonValue));
            }
            return result;
        }

        private async Task<TEntity> UpdateAsync(TEntity entity, ICandidate<TEntity, TKey> candidate, Query<TEntity> query)
        {
            await OnBeforeUpdateEntity(entity, candidate);

            entity = Patcher.Patch(entity, candidate.JsonValue);

            await OnAfterUpdateEntity(entity, candidate, query);

            await ValidateOrDiscardAsync(entity);

            return entity;
        }
    }
}
