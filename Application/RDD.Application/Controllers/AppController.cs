using NExtends.Primitives.Types;
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

		public async virtual Task<TEntity> CreateAsync(PostedData datas, Query<TEntity> query)
		{
			var entity = await _collection.CreateAsync(datas, query);

			await _storage.SaveChangesAsync();

			query.Options.NeedFilterRights = false;

			entity = await _collection.GetByIdAsync(entity.Id, query);

			return entity;
		}

		public async virtual Task<TEntity> UpdateAsync(TKey id, PostedData datas, Query<TEntity> query)
		{
			var entity = await GetByIdAsync(id, new Query<TEntity> { Verb = HttpVerb.PUT });

			entity = await _collection.UpdateAsync(entity, datas, query);

			await _storage.SaveChangesAsync();

			query.Options.NeedFilterRights = false;

			entity = await _collection.GetByIdAsync(entity.Id, query);

			return entity;
		}

		public async Task<IEnumerable<TEntity>> UpdateAsync(IDictionary<TKey, PostedData> datasByIds, Query<TEntity> query)
		{
			var ids = datasByIds.Keys.ToList();
			var entities = (await _collection.GetByIdsAsync(ids, new Query<TEntity> { Verb = HttpVerb.PUT }))
				.ToDictionary(el => el.Id, el => el);

			foreach (var kvp in datasByIds)
			{
				var entity = entities[kvp.Key];

				await _collection.UpdateAsync(entity, kvp.Value, query);
			}

			await _storage.SaveChangesAsync();

			query.Options.NeedFilterRights = false;

			var result = await _collection.GetByIdsAsync(ids, query);

			return result;
		}

		public async Task DeleteAsync(TKey id)
		{
			var entity = await _collection.GetByIdAsync(id, new Query<TEntity> { Verb = HttpVerb.DELETE });

			await _collection.DeleteAsync(entity);

			await _storage.SaveChangesAsync();
		}

		public async Task DeleteAsync(IList<TKey> ids)
		{
			var entities = await _collection.GetByIdsAsync(ids, new Query<TEntity> { Verb = HttpVerb.DELETE });

			foreach (var entity in entities)
			{
				await _collection.DeleteAsync(entity);
			}

			await _storage.SaveChangesAsync();
		}
	}
}
