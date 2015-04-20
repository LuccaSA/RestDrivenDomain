using RDD.Infra;
using RDD.Infra.Models.Querying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain
{
	public interface IRestDomainService<TEntity, TKey> : IRestService<TEntity, TKey>
		where TEntity : class, IEntityBase<TKey>
		where TKey : IEquatable<TKey>
	{
		void Create(TEntity entity);
		void CreateRange(IEnumerable<TEntity> entities);
		void Delete(TEntity entity);
		void DeleteRange(IEnumerable<TEntity> entities);

		IQueryable<TEntity> OrderByDefault(IQueryable<TEntity> entities);
		IQueryable<TEntity> OrderBy(IQueryable<TEntity> entities, string field, SortDirection direction, bool isFirst = true);

		Expression<Func<TEntity, bool>> Equals(string field, object value);
		Expression<Func<TEntity, bool>> NotEqual(string field, object value);
		Expression<Func<TEntity, bool>> Starts(string field, object value);
		Expression<Func<TEntity, bool>> Like(string field, object value);

		Expression<Func<TEntity, bool>> Between(string field, object value);
		Expression<Func<TEntity, bool>> Until(string field, object value);
		Expression<Func<TEntity, bool>> Since(string field, object value);

		Expression<Func<TEntity, bool>> GreaterThan(string field, object value);
		Expression<Func<TEntity, bool>> GreaterThanOrEqual(string field, object value);
		Expression<Func<TEntity, bool>> LessThan(string field, object value);
		Expression<Func<TEntity, bool>> LessThanOrEqual(string field, object value);
	}
}
