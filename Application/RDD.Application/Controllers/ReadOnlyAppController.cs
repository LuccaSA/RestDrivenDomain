using RDD.Domain;
using RDD.Domain.Models.Querying;
using System;
using System.Threading.Tasks;

namespace RDD.Application.Controllers
{
    public class ReadOnlyAppController<TCollection, TEntity, TKey> : IReadOnlyAppController<TCollection, TEntity, TKey>
		where TCollection : IReadOnlyRestCollection<TEntity, TKey>
		where TEntity : class, IEntityBase<TEntity, TKey>, new()
		where TKey : IEquatable<TKey>
	{
		protected TCollection _collection;

		public ReadOnlyAppController(TCollection collection)
		{
			_collection = collection;
		}

		public virtual async Task<ISelection<TEntity>> GetAsync(Query<TEntity> query)
		{
			var selection = await _collection.GetAsync(query);

			return selection;
		}

		public virtual async Task<TEntity> GetByIdAsync(TKey id, Query<TEntity> query)
		{
			var entity = await _collection.GetByIdAsync(id, query);

			return entity;
		}
	}
}
