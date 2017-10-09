using RDD.Domain.Helpers;
using RDD.Domain.Models;
using RDD.Domain.Models.Querying;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain
{
	public interface IRestCollection<TEntity> : IReadOnlyRestCollection<TEntity>
		where TEntity : class, IEntityBase
	{
		Task<TEntity> CreateAsync(object datas, Query<TEntity> query = null);
		Task<TEntity> CreateAsync(PostedData datas, Query<TEntity> query = null);
		Task CreateAsync(TEntity entity, Query<TEntity> query = null);
		Task DeleteAsync(TEntity entity);
	}

	public interface IRestCollection<TEntity, TKey> : IReadOnlyRestCollection<TEntity, TKey>, IRestCollection<TEntity>
		where TEntity : class, IEntityBase<TKey>
		where TKey : IEquatable<TKey>
	{
		Task<TEntity> UpdateAsync(TEntity entity, PostedData datas, Query<TEntity> query = null);
	}
}
