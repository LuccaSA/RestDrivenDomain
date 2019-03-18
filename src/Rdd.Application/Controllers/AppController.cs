using Rdd.Domain;
using Rdd.Domain.Models.Querying;
using System;
using System.Collections.Generic;
using System.Threading;
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

        public virtual async Task<TEntity> CreateAsync(ICandidate<TEntity, TKey> candidate, Query<TEntity> query, CancellationToken cancellationToken = default)
        {
            var result = await Collection.CreateAsync(candidate, query);
            await SaveChangesAsync(cancellationToken);
            return result;
        }

        public virtual async Task<IEnumerable<TEntity>> CreateAsync(IEnumerable<ICandidate<TEntity, TKey>> candidates, Query<TEntity> query, CancellationToken cancellationToken = default)
        {
            var result = await Collection.CreateAsync(candidates, query);
            await SaveChangesAsync(cancellationToken);
            return result;
        }

        public virtual async Task<IEnumerable<TEntity>> CreateAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            var result = await Collection.CreateAsync(entities);
            await SaveChangesAsync(cancellationToken);
            return result;
        }

        public virtual async Task<TEntity> UpdateByIdAsync(TKey id, ICandidate<TEntity, TKey> candidate, Query<TEntity> query, CancellationToken cancellationToken = default)
        {
            var result = await Collection.UpdateByIdAsync(id, candidate, query, cancellationToken);
            await SaveChangesAsync(cancellationToken);
            return result;
        }

        public virtual async Task<IEnumerable<TEntity>> UpdateByIdsAsync(IDictionary<TKey, ICandidate<TEntity, TKey>> candidatesByIds, Query<TEntity> query, CancellationToken cancellationToken = default)
        {
            var result = await Collection.UpdateByIdsAsync(candidatesByIds, query, cancellationToken);
            await SaveChangesAsync(cancellationToken);
            return result;
        }

        public virtual async Task DeleteByIdAsync(TKey id, CancellationToken cancellationToken = default)
        {
            await Collection.DeleteByIdAsync(id, cancellationToken);
            await SaveChangesAsync(cancellationToken);
        }

        public virtual async Task DeleteByIdsAsync(IEnumerable<TKey> ids, CancellationToken cancellationToken = default)
        {
            await Collection.DeleteByIdsAsync(ids, cancellationToken);
            await SaveChangesAsync(cancellationToken);
        }
         
        protected virtual async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}