using RDD.Domain.Models.Querying;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RDD.Domain.Models.Collections
{
	public interface IReadOnlyRestCollection<TEntity>
		where TEntity : class, IEntityBase
	{
		ISelection<TEntity> Get(Query<TEntity> query);
		IEnumerable<TEntity> GetAll();
	}

	public interface IReadOnlyRestCollection<TEntity, TKey> : IReadOnlyRestCollection<TEntity>
		where TEntity : class, IEntityBase<TKey>
	{
		TEntity GetById(TKey id);
		TEntity GetById(TKey id, Query<TEntity> query);

		IEnumerable<TEntity> GetByIds(ISet<TKey> ids);
		IEnumerable<TEntity> GetByIds(ISet<TKey> ids, Query<TEntity> query);
	}
}
