using Rdd.Domain;
using Rdd.Domain.Models.Querying;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

    public class AppController<TCollection, TEntity, TKey> : IAppController<TEntity, TKey>
        where TCollection : IRestCollection<TEntity, TKey>
        where TEntity : class, IEntityBase<TKey>
        where TKey : IEquatable<TKey>
    {
        private readonly IUnitOfWork _unitOfWork;
        protected readonly IRestCollection<TEntity, TKey> Collection;

        public AppController(IUnitOfWork unitOfWork, TCollection collection)
        {
            _unitOfWork = unitOfWork;
            Collection = collection;
        }

        public virtual async Task<TEntity> CreateAsync(ICandidate<TEntity, TKey> candidate)
        {
            var entity = await Collection.CreateAsync(candidate);
            await SaveChangesAsync();
            return entity;
        }

        public virtual async Task<IEnumerable<TEntity>> CreateAsync(IEnumerable<ICandidate<TEntity, TKey>> candidates)
        {
            var entities = await Collection.CreateAsync(candidates);
            await SaveChangesAsync();
            return entities;
        }

        public virtual async Task<TEntity> UpdateByIdAsync(TKey id, ICandidate<TEntity, TKey> candidate)
        {
            var entity = await Collection.UpdateByIdAsync(id, candidate);
            await SaveChangesAsync();
            return entity;
        }

        public virtual async Task<IEnumerable<TEntity>> UpdateByIdsAsync(IDictionary<TKey, ICandidate<TEntity, TKey>> candidatesByIds)
        {
            var entities = await Collection.UpdateByIdsAsync(candidatesByIds);
            await SaveChangesAsync();
            return entities;
        }

        public async Task DeleteByIdAsync(TKey id)
        {
            await Collection.DeleteByIdAsync(id);
            await SaveChangesAsync();
        }

        public async Task DeleteByIdsAsync(IEnumerable<TKey> ids)
        {
            await Collection.DeleteByIdsAsync(ids);
            await SaveChangesAsync();
        }
         
        protected virtual async Task SaveChangesAsync()
        {
            await _unitOfWork.SaveChangesAsync();
        }
    }
}