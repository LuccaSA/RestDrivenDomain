using RDD.Domain;
using RDD.Domain.Models.Querying;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RDD.Application
{
    public interface IAppController<TEntity, TKey> : IReadOnlyAppController<TEntity, TKey>
        where TEntity : class, IEntityBase<TEntity, TKey>
        where TKey : IEquatable<TKey>
    {
        Task<TEntity> CreateAsync(PostedData datas, Query<TEntity> query);
        Task<TEntity> UpdateByIdAsync(TKey id, PostedData datas, Query<TEntity> query);
        Task<IEnumerable<TEntity>> UpdateByIdsAsync(IDictionary<TKey, PostedData> datasByIds, Query<TEntity> query);
        Task DeleteByIdAsync(TKey id, Query<TEntity> query);
        Task DeleteByIdsAsync(IList<TKey> ids, Query<TEntity> query);
    }
}
