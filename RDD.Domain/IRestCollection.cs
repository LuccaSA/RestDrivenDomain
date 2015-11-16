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
	public interface IRestCollection<TEntity>
		where TEntity : class, IEntityBase
	{
		ICollection<TEntity> Items { get; }
		int Count { get; }

		IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> filter, HttpVerb verb = HttpVerb.GET);
		IRestCollection<TEntity> Get(Query<TEntity> query, HttpVerb verb = HttpVerb.GET);
		List<TEntity> GetAll();

		TEntity Create(object datas, Query<TEntity> query = null);
		TEntity Create(PostedData datas, Query<TEntity> query = null);
		void Create(TEntity entity, Query<TEntity> query = null);
		void CreateRange(IEnumerable<TEntity> entities, Query<TEntity> query = null);

		void Delete(TEntity entity);
		void DeleteRange(IEnumerable<TEntity> entities);

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

	public interface IRestCollection<TEntity, TKey> : IRestCollection<TEntity>
		where TEntity : class, IEntityBase<TKey>
		where TKey : IEquatable<TKey>
	{
		TEntity GetById(TKey id, HttpVerb verb = HttpVerb.GET);
		TEntity GetById(TKey id, Query<TEntity> query, HttpVerb verb = HttpVerb.GET);
		ICollection<TEntity> GetByIds(ISet<TKey> ids, HttpVerb verb = HttpVerb.GET);
		ICollection<TEntity> GetByIds(ISet<TKey> ids, Query<TEntity> query, HttpVerb verb = HttpVerb.GET);

		TEntity Update(TKey id, PostedData datas, Query<TEntity> query = null);
		TEntity Update(TKey id, object datas, Query<TEntity> query = null);
	
		void Delete(TKey id);
	}
}
