using Rdd.Domain;
using Rdd.Domain.Models.Querying;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rdd.Domain.Helpers;

namespace Rdd.Application.Controllers
{
    public class ReadOnlyAppController<TEntity, TKey> : ReadOnlyAppController<IReadOnlyRestCollection<TEntity, TKey>, TEntity, TKey>
        where TEntity : class, IEntityBase<TKey>
        where TKey : IEquatable<TKey>
    {
        public ReadOnlyAppController(IReadOnlyRestCollection<TEntity, TKey> collection) 
            : base(collection)
        {
        }
    }

    public class ReadOnlyAppController<TCollection, TEntity, TKey> : IReadOnlyAppController<TEntity, TKey>
        where TCollection : IReadOnlyRestCollection<TEntity, TKey>
        where TEntity : class, IEntityBase<TKey>
        where TKey : IEquatable<TKey>
    {
        protected TCollection Collection { get; }

        public ReadOnlyAppController(TCollection collection)
        {
            Collection = collection;
        }

        public virtual async Task<ISelection<TEntity>> GetAsync(Query<TEntity> query)
        {
            var entities = await Collection.GetAsync(query);
            await OnAfterGetAsync(entities.Items);
            return entities;
        }

        public virtual async Task<TEntity> GetByIdAsync(TKey id, Query<TEntity> query)
        {
            var entity = await Collection.GetByIdAsync(id, query);
            await OnAfterGetAsync(entity.Yield());
            return entity;
        }

        /// <summary>
        /// Called after all Get methods, should be used to apply custom modifications before items are returned via API 
        /// </summary>
        protected virtual Task OnAfterGetAsync(IEnumerable<TEntity> entities) => Task.CompletedTask;
    }
}
