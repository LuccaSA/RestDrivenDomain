using RDD.Domain;
using RDD.Domain.Models.Querying;
using RDD.Infra;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RDD.Domain.Helpers;

namespace RDD.Application.Controllers
{
    public class AppController<TEntity, TKey> : AppController<IRestCollection<TEntity, TKey>, TEntity, TKey>
        where TEntity : class, IEntityBase<TEntity, TKey>
        where TKey : IEquatable<TKey>
    {
        public AppController(IStorageService storage, IRestCollection<TEntity, TKey> collection)
            : base(storage, collection)
        {
        }
    }

    public class AppController<TCollection, TEntity, TKey> : ReadOnlyAppController<TCollection, TEntity, TKey>, IAppController<TEntity, TKey>
        where TCollection : IRestCollection<TEntity, TKey>
        where TEntity : class, IEntityBase<TEntity, TKey>
        where TKey : IEquatable<TKey>
    {
        protected IStorageService Storage { get; }

        public AppController(IStorageService storage, TCollection collection)
            : base(collection)
        {
            Storage = storage;
        }

        public virtual async Task<TEntity> CreateAsync(ICandidate<TEntity, TKey> candidate, Query<TEntity> query)
        {
            var entity = await Collection.CreateAsync(candidate, query);
            await OnBeforeCreateAsync(entity.Yield());
            await Storage.SaveChangesAsync();
            await OnAfterCreateAsync(entity.Yield());
            return entity;
        }

        public virtual async Task<TEntity> CreateAsync(TEntity candidateEntity)
        {
            var entity = await Collection.CreateAsync(candidateEntity);
            await OnBeforeCreateAsync(entity.Yield());
            await Storage.SaveChangesAsync();
            await OnAfterCreateAsync(entity.Yield());
            return entity;
        }

        public virtual async Task<IEnumerable<TEntity>> CreateAsync(IEnumerable<ICandidate<TEntity, TKey>> candidates, Query<TEntity> query)
        {
            var entities = await Collection.CreateAsync(candidates, query);

            await Storage.SaveChangesAsync();

            return entities;
        }

        public virtual async Task<TEntity> UpdateByIdAsync(TKey id, ICandidate<TEntity, TKey> candidate, Query<TEntity> query)
        {
            var entity = await Collection.UpdateByIdAsync(id, candidate, query);
            await OnBeforeUpdateAsync(entity.Yield());
            await Storage.SaveChangesAsync();

            await OnAfterUpdateAsync(entity.Yield());
            return entity;
        }

        public virtual async Task<TEntity> UpdateByIdAsync(TKey id, TEntity candidateEntity)
        {
            var entity = await Collection.UpdateByIdAsync(id, candidateEntity);
            await OnBeforeUpdateAsync(entity.Yield());
            await Storage.SaveChangesAsync();
            await OnAfterUpdateAsync(entity.Yield());
            return entity;
        }

        public async Task<IEnumerable<TEntity>> UpdateByIdsAsync(IDictionary<TKey, ICandidate<TEntity, TKey>> candidatesByIds, Query<TEntity> query)
        {
            var entities = await Collection.UpdateByIdsAsync(candidatesByIds, query);
            await OnBeforeUpdateAsync(entities);
            await Storage.SaveChangesAsync();
            await OnAfterUpdateAsync(entities);
            return entities;
        }

        protected virtual Task OnBeforeCreateAsync(IEnumerable<TEntity> entity) => Task.CompletedTask;
        protected virtual Task OnBeforeUpdateAsync(IEnumerable<TEntity> entity) => Task.CompletedTask;

        protected virtual Task OnAfterCreateAsync(IEnumerable<TEntity> entity) => Task.CompletedTask;
        protected virtual Task OnAfterUpdateAsync(IEnumerable<TEntity> entity) => Task.CompletedTask;

        public async Task DeleteByIdAsync(TKey id)
        {
            await Collection.DeleteByIdAsync(id);

            await Storage.SaveChangesAsync();
        }

        public async Task DeleteByIdsAsync(IEnumerable<TKey> ids)
        {
            await Collection.DeleteByIdsAsync(ids);

            await Storage.SaveChangesAsync();
        }
    }
}