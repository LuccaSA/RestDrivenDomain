﻿using Rdd.Domain;
using Rdd.Domain.Models.Querying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rdd.Domain.Helpers;

namespace Rdd.Application.Controllers
{
    public class AppController<TEntity, TKey> : AppController<IRestCollection<TEntity, TKey>, TEntity, TKey>
        where TEntity : class, IEntityBase<TKey>
        where TKey : IEquatable<TKey>
    {
        public AppController(IUnitOfWork unitOfWork, IRestCollection<TEntity, TKey> collection)
            : base(unitOfWork, collection)
        {
        }
    }

    public class AppController<TCollection, TEntity, TKey> : ReadOnlyAppController<TCollection, TEntity, TKey>, IAppController<TEntity, TKey>
        where TCollection : IRestCollection<TEntity, TKey>
        where TEntity : class, IEntityBase<TKey>
        where TKey : IEquatable<TKey>
    {
        private readonly IUnitOfWork _unitOfWork;

        public AppController(IUnitOfWork unitOfWork, TCollection collection)
            : base(collection)
        {
            _unitOfWork = unitOfWork;
        }

        public virtual async Task<TEntity> CreateAsync(ICandidate<TEntity, TKey> candidate, Query<TEntity> query)
        {
            var entity = await Collection.CreateAsync(candidate, query);
            await SaveChangesAsync(entity.Yield());
            return entity;
        }

        public virtual async Task<IEnumerable<TEntity>> CreateAsync(IEnumerable<ICandidate<TEntity, TKey>> candidates, Query<TEntity> query)
        {
            var entities = await Collection.CreateAsync(candidates, query);
            await SaveChangesAsync(entities);
            return entities;
        }

        public virtual async Task<TEntity> UpdateByIdAsync(TKey id, ICandidate<TEntity, TKey> candidate, Query<TEntity> query)
        {
            var entity = await Collection.UpdateByIdAsync(id, candidate, query);
            await SaveChangesAsync(entity.Yield());
            return entity;
        }

        public virtual async Task<IEnumerable<TEntity>> UpdateByIdsAsync(IDictionary<TKey, ICandidate<TEntity, TKey>> candidatesByIds, Query<TEntity> query)
        {
            var entities = await Collection.UpdateByIdsAsync(candidatesByIds, query);
            await SaveChangesAsync(entities);
            return entities;
        }

        public async Task DeleteByIdAsync(TKey id)
        {
            await Collection.DeleteByIdAsync(id);
            await SaveChangesAsync(Enumerable.Empty<TEntity>());
        }

        public async Task DeleteByIdsAsync(IEnumerable<TKey> ids)
        {
            await Collection.DeleteByIdsAsync(ids);
            await SaveChangesAsync(Enumerable.Empty<TEntity>());
        }

        /// <summary>
        /// Calls UnitOfWork.SaveChangesAsync() and pass modified items to OnBefore / OnAfter methods
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        protected virtual async Task SaveChangesAsync(IEnumerable<TEntity> entities)
        {
            await OnBeforeSaveEntitiesAsync(entities);
            await _unitOfWork.SaveChangesAsync();
            await OnAfterSaveEntitiesAsync(entities);
        }

        /// <summary>
        /// Called before SaveChangesAsync(), last opportunity to modify entities
        /// </summary>
        protected virtual Task OnBeforeSaveEntitiesAsync(IEnumerable<TEntity> entities) => Task.CompletedTask;

        /// <summary>
        /// Called after SaveChangesAsync(), should be used to apply custom modifications before items are returned via API
        /// </summary>
        protected virtual Task OnAfterSaveEntitiesAsync(IEnumerable<TEntity> entities) => Task.CompletedTask;
    }
}