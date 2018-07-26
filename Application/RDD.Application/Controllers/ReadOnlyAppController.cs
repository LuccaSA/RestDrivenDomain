using RDD.Domain;
using RDD.Domain.Models.Querying;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RDD.Application.Controllers
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

        public virtual Task<IReadOnlyCollection<TEntity>> GetAsync(Query<TEntity> query) => Collection.GetAsync(query);

        public virtual Task<TEntity> GetByIdAsync(TKey id, Query<TEntity> query) => Collection.GetByIdAsync(id, query);
    }
}
