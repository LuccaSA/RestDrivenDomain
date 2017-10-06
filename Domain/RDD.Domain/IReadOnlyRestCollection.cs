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
	public interface IReadOnlyRestCollection<TEntity>
		where TEntity : class, IEntityBase
	{
		Task<ISelection<TEntity>> GetAsync(Query<TEntity> query);
		Task<IEnumerable<TEntity>> GetAllAsync();

		Task<bool> AnyAsync(Query<TEntity> query);
	}

	public interface IReadOnlyRestCollection<TEntity, TKey> : IReadOnlyRestCollection<TEntity>
		where TEntity : class, IEntityBase<TKey>
		where TKey : IEquatable<TKey>
	{
		Task<TEntity> GetByIdAsync(TKey id, HttpVerb verb);
		Task<TEntity> GetByIdAsync(TKey id, Query<TEntity> query);
		Task<IEnumerable<TEntity>> GetByIdsAsync(IList<TKey> ids, HttpVerb verb);
		Task<IEnumerable<TEntity>> GetByIdsAsync(IList<TKey> ids, Query<TEntity> query);
	}
}
