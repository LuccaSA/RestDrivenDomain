using RDD.Domain.Models.Querying;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RDD.Domain
{
	public interface IRestCollection<TEntity> : IReadOnlyRestCollection<TEntity>
		where TEntity : class, IEntityBase
	{
		Task<TEntity> CreateAsync(object datas, Query<TEntity> query = null);
		Task<TEntity> CreateAsync(PostedData datas, Query<TEntity> query = null);
        Task<TEntity> CreateAsync(TEntity entity, Query<TEntity> query = null);
	}

	public interface IRestCollection<TEntity, TKey> : IReadOnlyRestCollection<TEntity, TKey>, IRestCollection<TEntity>
		where TEntity : class, IEntityBase<TKey>
		where TKey : IEquatable<TKey>
	{
        Task<TEntity> UpdateByIdAsync(TKey id, object datas, Query<TEntity> query = null);
        Task<TEntity> UpdateByIdAsync(TKey id, PostedData datas, Query<TEntity> query = null);
        Task<IEnumerable<TEntity>> UpdateByIdsAsync(IDictionary<TKey, PostedData> datasByIds, Query<TEntity> query = null);

        Task DeleteByIdAsync(TKey id);
        Task DeleteByIdsAsync(IList<TKey> ids);
    }
}
