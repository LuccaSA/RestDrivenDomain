using RDD.Domain;
using RDD.Domain.Models.Querying;
using System;
using System.Threading.Tasks;

namespace RDD.Application
{
	public interface IReadOnlyAppController<TCollection, TEntity, TKey> : IReadOnlyAppController<TEntity, TKey>
		where TCollection : IReadOnlyRestCollection<TEntity, TKey>
		where TEntity : class, IEntityBase<TEntity, TKey>, new()
		where TKey : IEquatable<TKey>
	{
	}

	public interface IReadOnlyAppController<TEntity, TKey>
		where TEntity : class, IEntityBase<TEntity, TKey>, new()
		where TKey : IEquatable<TKey>
	{
		Task<ISelection<TEntity>> GetAsync(Query<TEntity> query);
		Task<TEntity> GetByIdAsync(TKey id, Query<TEntity> query);
	}
}
