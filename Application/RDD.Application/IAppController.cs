using RDD.Domain;
using RDD.Domain.Models.Querying;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RDD.Application
{
	public interface IAppController<TCollection, TEntity, TKey> : IReadOnlyAppController<TCollection, TEntity, TKey>, IAppController<TEntity, TKey>
		where TCollection : IRestCollection<TEntity, TKey>
		where TEntity : class, IEntityBase<TEntity, TKey>, new()
		where TKey : IEquatable<TKey>
	{
	}

	public interface IAppController<TEntity, TKey> : IReadOnlyAppController<TEntity, TKey>
		where TEntity : class, IEntityBase<TEntity, TKey>, new()
		where TKey : IEquatable<TKey>
	{
		Task<TEntity> CreateAsync(PostedData datas, Query<TEntity> query);
		Task<TEntity> UpdateAsync(TKey id, PostedData datas, Query<TEntity> query);
		Task<IEnumerable<TEntity>> UpdateAsync(IDictionary<TKey, PostedData> datasByIds, Query<TEntity> query);
		Task DeleteAsync(TKey id);
		Task DeleteAsync(IList<TKey> ids);
	}
}
