using RDD.Domain.Models.Querying;
using System;
using System.Collections.Generic;

namespace RDD.Domain.Models.Collections
{
	public interface IRestCollection<TEntity> : IReadOnlyRestCollection<TEntity>
		where TEntity : class, IEntityBase
	{
		TEntity Create(PostedData datas, Query<TEntity> query = null);
		void Create(TEntity entity, Query<TEntity> query = null);
		TEntity GetEntityAfterCreate(TEntity entity, Query<TEntity> query = null);
		void CreateRange(IEnumerable<TEntity> entities, Query<TEntity> query = null);

		TEntity Update(TEntity entity, PostedData datas, Query<TEntity> query = null);

		void Delete(TEntity entity);
		void DeleteRange(IEnumerable<TEntity> entities);
	}

	public interface IRestCollection<TEntity, TKey> : IRestCollection<TEntity>, IReadOnlyRestCollection<TEntity, TKey>
		where TEntity : class, IEntityBase<TKey>
	{
		TEntity Update(TKey id, PostedData datas, Query<TEntity> query = null);

		void Delete(TKey id);
	}
}
