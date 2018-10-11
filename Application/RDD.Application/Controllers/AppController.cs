using Rdd.Domain;
using Rdd.Domain.Models.Querying;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rdd.Application.Controllers
{
    public class AppController<TEntity, TKey> : AppController<IRestCollection<TEntity, TKey>, TEntity, TKey>
        where TEntity : class, IEntityBase<TEntity, TKey>
        where TKey : IEquatable<TKey>
    {
        public AppController(IUnitOfWork unitOfWork, IRestCollection<TEntity, TKey> collection)
            : base(unitOfWork, collection)
        {
        }
    }

    public class AppController<TCollection, TEntity, TKey> : ReadOnlyAppController<TCollection, TEntity, TKey>, IAppController<TEntity, TKey>
        where TCollection : IRestCollection<TEntity, TKey>
        where TEntity : class, IEntityBase<TEntity, TKey>
        where TKey : IEquatable<TKey>
    {
        protected IUnitOfWork UnitOfWork { get; }

        public AppController(IUnitOfWork unitOfWork, TCollection collection)
            : base(collection)
        {
            UnitOfWork = unitOfWork;
        }

        public virtual async Task<TEntity> CreateAsync(ICandidate<TEntity, TKey> candidate, Query<TEntity> query)
        {
            var entity = await Collection.CreateAsync(candidate, query);

            await UnitOfWork.SaveChangesAsync();

            return entity;
        }

        public virtual async Task<IEnumerable<TEntity>> CreateAsync(IEnumerable<ICandidate<TEntity, TKey>> candidates, Query<TEntity> query)
        {
            var entities = await Collection.CreateAsync(candidates, query);

            await UnitOfWork.SaveChangesAsync();

            return entities;
        }

        public virtual async Task<TEntity> UpdateByIdAsync(TKey id, ICandidate<TEntity, TKey> candidate, Query<TEntity> query)
        {
            var entity = await Collection.UpdateByIdAsync(id, candidate, query);

            await UnitOfWork.SaveChangesAsync();

            return entity;
        }

        public virtual async Task<IEnumerable<TEntity>> UpdateByIdsAsync(IDictionary<TKey, ICandidate<TEntity, TKey>> candidatesByIds, Query<TEntity> query)
        {
            var entities = await Collection.UpdateByIdsAsync(candidatesByIds, query);

            await UnitOfWork.SaveChangesAsync();

            return entities;
        }

        public async Task DeleteByIdAsync(TKey id)
        {
            await Collection.DeleteByIdAsync(id);

            await UnitOfWork.SaveChangesAsync();
        }

        public async Task DeleteByIdsAsync(IEnumerable<TKey> ids)
        {
            await Collection.DeleteByIdsAsync(ids);

            await UnitOfWork.SaveChangesAsync();
        }
    }
}