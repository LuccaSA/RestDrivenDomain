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
        private readonly IPatcher<TEntity> _patcher;
        private readonly IRepository<TEntity> _repository;
        private readonly IInstanciator<TEntity> _instanciator;

        public RestCollection(IRepository<TEntity> repository, IPatcher<TEntity> patcher, IInstanciator<TEntity> instanciator)
            : base(repository)
        {
            _patcher = patcher;
            _repository = repository;
            _instanciator = instanciator;
        }

        public virtual Task<TEntity> CreateAsync(ICandidate<TEntity, TKey> candidate, Query<TEntity> query = null)
        {
            TEntity entity = _instanciator.InstanciateNew(candidate);

            _patcher.Patch(entity, candidate.JsonValue);

            return CreateAsync(entity, query);
        }

        public virtual Task<TEntity> CreateAsync(TEntity entity, Query<TEntity> query = null)
        {
            ForgeEntity(entity);

            ValidateEntity(entity, null);

            _repository.Add(entity);

            return Task.FromResult(entity);
        }

        public virtual async Task<IReadOnlyCollection<TEntity>> CreateAsync(IEnumerable<ICandidate<TEntity, TKey>> candidates, Query<TEntity> query = null)
        {
            var result = new List<TEntity>();

            foreach (var candidate in candidates)
            {
                TEntity entity = _instanciator.InstanciateNew(candidate);

                _patcher.Patch(entity, candidate.JsonValue);

                ForgeEntity(entity);

                ValidateEntity(entity, null);

                result.Add(entity);
            }

            _repository.AddRange(result);
            return result;
        }

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

        public virtual async Task<IReadOnlyCollection<TEntity>> UpdateByIdsAsync(IDictionary<TKey, ICandidate<TEntity, TKey>> candidatesByIds, Query<TEntity> query = null)
        {
            query = query ?? new Query<TEntity>();
            query.Verb = HttpVerbs.Put;

            var result = new List<TEntity>();

            var ids = candidatesByIds.Select(d => d.Key).ToList();
            var expQuery = new Query<TEntity>(query, e => ids.Contains(e.Id));
            var entities = (await GetAsync(expQuery)).ToDictionary(el => el.Id);

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
                _repository.Remove(entity);
            }
        }

        public virtual async Task DeleteByIdsAsync(IEnumerable<TKey> ids)
        {
            var expQuery = new Query<TEntity>(e => ids.Contains(e.Id))
            {
                Verb = HttpVerbs.Delete
            };

            var entities = (await GetAsync(expQuery)).ToDictionary(el => el.Id);

            foreach (TKey id in ids)
            {
                var entity = entities[id];

                _repository.Remove(entity);
            }
        }

        protected virtual void ForgeEntity(TEntity entity) { }

        protected virtual void ValidateEntity(TEntity entity, TEntity oldEntity) { }

        protected virtual Task OnBeforeUpdateEntity(TEntity entity, ICandidate<TEntity, TKey> candidate) => Task.CompletedTask;

        /// <summary>
        /// Called after entity update
        /// As "oldEntity" is a MemberWiseClone of "entity" before its update, it's a one level deep copy. If you want to go deeper
        /// you can do it by overriding the Clone() method and MemberWiseClone individual sub-properties
        /// </summary>
        protected virtual Task OnAfterUpdateEntity(TEntity oldEntity, TEntity entity, ICandidate<TEntity, TKey> candidate, Query<TEntity> query) => Task.CompletedTask;

        private async Task<TEntity> UpdateAsync(TEntity entity, ICandidate<TEntity, TKey> candidate, Query<TEntity> query)
        {
            await OnBeforeUpdateEntity(entity, candidate);

            TEntity oldEntity = entity.Clone();

            _patcher.Patch(entity, candidate.JsonValue);

            await OnAfterUpdateEntity(oldEntity, entity, candidate, query);

            ValidateEntity(entity, oldEntity);

            return entity;
        }
    }
}