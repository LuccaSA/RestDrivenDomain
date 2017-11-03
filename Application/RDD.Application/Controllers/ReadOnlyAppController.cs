using RDD.Domain;
using RDD.Domain.Models.Querying;
using System;
using System.Threading.Tasks;

namespace RDD.Application.Controllers
{
    public class ReadOnlyAppController<TEntity, TKey> : ReadOnlyAppController<IReadOnlyRestCollection<TEntity, TKey>, TEntity, TKey>
        where TEntity : class, IEntityBase<TEntity, TKey>, new()
        where TKey : IEquatable<TKey>
    {
        public ReadOnlyAppController(IReadOnlyRestCollection<TEntity, TKey> collection) 
            : base(collection)
        {
        }
    }

    public class ReadOnlyAppController<TCollection, TEntity, TKey> : IReadOnlyAppController<TEntity, TKey>
        where TCollection : IReadOnlyRestCollection<TEntity, TKey>
        where TEntity : class, IEntityBase<TEntity, TKey>, new()
        where TKey : IEquatable<TKey>
    {
        protected TCollection Collection { get; }

        public ReadOnlyAppController(TCollection collection)
        {
            Collection = collection;
        }

        public virtual async Task<ISelection<TEntity>> GetAsync(Query<TEntity> query)
        {
            var selection = await Collection.GetAsync(query);

            return selection;
        }

        public virtual async Task<TEntity> GetByIdAsync(TKey id, Query<TEntity> query)
        {
            var entity = await Collection.GetByIdAsync(id, query);

            return entity;
        }
    }
}
