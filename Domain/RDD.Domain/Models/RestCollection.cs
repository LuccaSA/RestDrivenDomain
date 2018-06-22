using Newtonsoft.Json;
using RDD.Domain.Exceptions;
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
        protected IPatcherProvider PatcherProvider { get; private set; }

        public RestCollection(IRepository<TEntity> repository, IExecutionContext execution, ICombinationsHolder combinationsHolder,
            IPatcherProvider patcherProvider)
            : base(repository, execution, combinationsHolder)
        {
            PatcherProvider = patcherProvider;
        }

        public virtual Task<TEntity> CreateAsync(ICandidate<TEntity, TKey> candidate, Query<TEntity> query = null)
        {
            TEntity entity = InstanciateEntity(candidate);

            GetPatcher().Patch(entity, candidate.JsonValue);

            return CreateAsync(entity, query);
        }

        public virtual async Task<TEntity> CreateAsync(TEntity entity, Query<TEntity> query = null)
        {
            if (query == null || query.Options.CheckRights)
            {
                await CheckRightsForCreateAsync(entity);
            }

            ForgeEntity(entity);

            ValidateEntity(entity, null);

            Repository.Add(entity);

            return entity;
        }

        public virtual async Task<TEntity> UpdateByIdAsync(TKey id, ICandidate<TEntity, TKey> candidate, Query<TEntity> query = null)
        {
            query = query ?? new Query<TEntity>();
            query.Verb = HttpVerbs.Put;
            query.Options.AttachActions = true;
            query.Options.AttachOperations = true;

            TEntity entity = await GetByIdAsync(id, query);

            return await UpdateAsync(entity, candidate, query);
        }

        public virtual async Task<IEnumerable<TEntity>> UpdateByIdsAsync(IDictionary<TKey, ICandidate<TEntity, TKey>> candidatesByIds, Query<TEntity> query = null)
        {
            query = query ?? new Query<TEntity>();
            query.Verb = HttpVerbs.Put;
            query.Options.AttachActions = true;
            query.Options.AttachOperations = true;

            var result = new HashSet<TEntity>();

            var ids = candidatesByIds.Select(d => d.Key).ToList();
            var expQuery = new Query<TEntity>(query, e => ids.Contains(e.Id));
            var entities = (await GetAsync(expQuery)).Items.ToDictionary(el => el.Id, el => el);

            foreach (KeyValuePair<TKey, ICandidate<TEntity, TKey>> kvp in candidatesByIds)
            {
                var entity = entities[kvp.Key];
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
            var expQuery = new Query<TEntity>(e => ids.Contains(e.Id));
            expQuery.Verb = HttpVerbs.Delete;

            var entities = (await GetAsync(expQuery)).Items.ToDictionary(el => el.Id, el => el);

            foreach (var id in ids)
            {
                var entity = entities[id];

                Repository.Remove(entity);
            }
        }

        /// <summary>
        /// We dropped the new() constraint for all these reasons
        /// https://blogs.msdn.microsoft.com/seteplia/2017/02/01/dissecting-the-new-constraint-in-c-a-perfect-example-of-a-leaky-abstraction/
        /// </summary>
        /// <returns></returns>
        public virtual TEntity InstanciateEntity(ICandidate<TEntity, TKey> candidate)
        {
            return System.Activator.CreateInstance<TEntity>();
        }

        protected virtual Task CheckRightsForCreateAsync(TEntity entity)
        {
            IEnumerable<int> operationIds = CombinationsHolder.Combinations
                .Where(c => c.Subject == typeof(TEntity) && c.Verb.HasVerb(HttpVerbs.Post))
                .Select(c => c.Operation.Id);

            if (!Execution.curPrincipal.HasAnyOperations(new HashSet<int>(operationIds)))
            {
                throw new UnauthorizedException(string.Format("You cannot create entity of type {0}", typeof(TEntity).Name));
            }

            return Task.CompletedTask;
        }

        protected virtual IPatcher GetPatcher() => new ObjectPatcher(PatcherProvider);

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

            GetPatcher().Patch(entity, candidate.JsonValue);

            await OnAfterUpdateEntity(oldEntity, entity, candidate, query);

            ValidateEntity(entity, oldEntity);

            return entity;
        }
    }
}