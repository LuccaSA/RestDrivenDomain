using RDD.Domain;
using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RDD.Application.Controllers
{
	public class AppController<TCollection, TEntity, TKey> : ReadOnlyAppController<TCollection, TEntity, TKey>, IAppController<TCollection, TEntity, TKey>
		where TCollection : IRestCollection<TEntity, TKey>
		where TEntity : class, IEntityBase<TEntity, TKey>, new()
		where TKey : IEquatable<TKey>
	{
		protected IStorageService _storage;

		public AppController(IStorageService storage, TCollection collection)
			: base(collection)
		{
			_storage = storage;
		}

		public virtual async Task<TEntity> CreateAsync(PostedData datas, Query<TEntity> query)
		{
			var entity = await _collection.CreateAsync(datas, query);

			await _storage.SaveChangesAsync();

			query.Options.NeedFilterRights = false;

			entity = await _collection.GetByIdAsync(entity.Id, query);

			return entity;
		}

		public virtual async Task<TEntity> UpdateByIdAsync(TKey id, PostedData datas, Query<TEntity> query)
		{
			var entity = await _collection.UpdateByIdAsync(id, datas, query);

			await _storage.SaveChangesAsync();

			query.Options.NeedFilterRights = false;
            query.Options.AttachActions = false;
            query.Options.AttachOperations = false;

            entity = await _collection.GetByIdAsync(id, query);

			return entity;
		}

		public async Task<IEnumerable<TEntity>> UpdateByIdsAsync(IDictionary<TKey, PostedData> datasByIds, Query<TEntity> query)
		{
            var entities = await _collection.UpdateByIdsAsync(datasByIds, query);

			await _storage.SaveChangesAsync();

			query.Options.NeedFilterRights = false;

            var ids = entities.Select(e => e.Id).ToList();
			var result = await _collection.GetByIdsAsync(ids, query);

			return result;
		}

		public async Task DeleteByIdAsync(TKey id)
		{
			await _collection.DeleteByIdAsync(id);

			await _storage.SaveChangesAsync();
		}

		public async Task DeleteByIdsAsync(IList<TKey> ids)
		{
			await _collection.DeleteByIdsAsync(ids);

			await _storage.SaveChangesAsync();
		}
	}
}
