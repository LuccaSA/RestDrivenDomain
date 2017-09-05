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
		PropertySelector<TEntity> HandleIncludes(PropertySelector<TEntity> includes, HttpVerb verb, Field<TEntity> fields);

		ISelection<TEntity> Get(Query<TEntity> query, HttpVerb verb = HttpVerb.GET);
		Task<ISelection<TEntity>> GetAsync(Query<TEntity> query, HttpVerb verb = HttpVerb.GET);

		IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> filter, HttpVerb verb = HttpVerb.GET);
		Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>> filter, HttpVerb verb = HttpVerb.GET);
		IEnumerable<TEntity> GetAll();

		IQueryable<TEntity> OrderByDefault(IQueryable<TEntity> entities);
		IQueryable<TEntity> OrderBy(IQueryable<TEntity> entities, string field, SortDirection direction, bool isFirst = true);

		Expression<Func<TEntity, bool>> Equals(string field, IList values);
		Expression<Func<TEntity, bool>> NotEqual(string field, IList values);
		Expression<Func<TEntity, bool>> Starts(string field, IList values);
		Expression<Func<TEntity, bool>> Like(string field, IList values);

		Expression<Func<TEntity, bool>> Between(string field, IList values);
		Expression<Func<TEntity, bool>> Until(string field, IList values);
		Expression<Func<TEntity, bool>> Since(string field, IList values);

		Expression<Func<TEntity, bool>> GreaterThan(string field, IList values);
		Expression<Func<TEntity, bool>> GreaterThanOrEqual(string field, IList values);
		Expression<Func<TEntity, bool>> LessThan(string field, IList values);
		Expression<Func<TEntity, bool>> LessThanOrEqual(string field, IList values);
	}

	public interface IReadOnlyRestCollection<TEntity, TKey> : IReadOnlyRestCollection<TEntity>
		where TEntity : class, IEntityBase<TKey>
		where TKey : IEquatable<TKey>
	{
		TEntity GetById(TKey id, HttpVerb verb = HttpVerb.GET);
		Task<TEntity> GetByIdAsync(TKey id, HttpVerb verb = HttpVerb.GET);
		TEntity GetById(TKey id, Query<TEntity> query, HttpVerb verb = HttpVerb.GET);
		Task<TEntity> GetByIdAsync(TKey id, Query<TEntity> query, HttpVerb verb = HttpVerb.GET);
		IEnumerable<TEntity> GetByIds(ISet<TKey> ids, HttpVerb verb = HttpVerb.GET);
		Task<IEnumerable<TEntity>> GetByIdsAsync(ISet<TKey> ids, HttpVerb verb = HttpVerb.GET);
		IEnumerable<TEntity> GetByIds(ISet<TKey> ids, Query<TEntity> query, HttpVerb verb = HttpVerb.GET);
		Task<IEnumerable<TEntity>> GetByIdsAsync(ISet<TKey> ids, Query<TEntity> query, HttpVerb verb = HttpVerb.GET);
	}
}
