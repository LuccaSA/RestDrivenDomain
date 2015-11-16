using LinqKit;
using RDD.Domain.Models.Querying;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain.Helpers
{
	public class PredicateService
	{
		private ICollection<Where> _wheres;

		public PredicateService(ICollection<Where> wheres)
		{
			_wheres = wheres;
		}

		public Expression<Func<TObject, bool>> GetPredicate<TObject>()
			where TObject : class
		{
			var feed = PredicateBuilder.True<TObject>();

			foreach (var where in _wheres)
			{
				var sub = PredicateBuilder.False<TObject>();

				foreach (var value in where.Values)
				{
					sub = sub.Or(ToExpression<TObject>(where, value).Expand());
				}

				feed = feed.And(sub.Expand());
			}

			return feed.Expand();
		}

		private Expression<Func<TObject, bool>> ToExpression<TObject>(Where where, object value)
			where TObject : class
		{
			var whereField = where.Field;
			var whereType = where.Type;

			switch (whereType)
			{
				case WhereOperand.Equals:

					var type = typeof(TObject);
					var property = type.GetProperties().Where(p => p.Name.ToLower() == whereField.ToLower()).FirstOrDefault();

					var parameter = Expression.Parameter(type, "entity");

					var body = Expression.PropertyOrField(parameter, whereField);

					return Expression.Lambda<Func<TObject, bool>>(Expression.Equal(body, Expression.Constant(value, property.PropertyType)), parameter);

				default:
					throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Fait des AND entre les where dont les différentes valeurs dont combinées via des OR
		/// /api/users?manager.id=2&departement.id=4,5 devient manager.id == 2 AND ( department.id == 4 OR department.id == 5 )
		/// </summary>
		/// <returns></returns>
		public Expression<Func<TEntity, bool>> GetEntityPredicate<TEntity, TKey>(IRestCollection<TEntity, TKey> repo)
			where TEntity : class, IEntityBase<TEntity, TKey>
			where TKey : IEquatable<TKey>
		{
			var feed = PredicateBuilder.True<TEntity>();

			foreach (var where in _wheres)
			{
				feed = feed.And(ToEntityExpression(repo, where, where.Values).Expand());
			}

			return feed.Expand();
		}

		private Expression<Func<TEntity, bool>> ToEntityExpression<TEntity, TKey>(IRestCollection<TEntity, TKey> repo, Where where, IList value)
			where TEntity : class, IEntityBase<TEntity, TKey>
			where TKey : IEquatable<TKey>
		{
			switch (where.Type)
			{
				case WhereOperand.Equals: return repo.Equals(where.Field, value);
				case WhereOperand.NotEqual: return repo.NotEqual(where.Field, value);
				case WhereOperand.Starts: return repo.Starts(where.Field, value);
				case WhereOperand.Like: return repo.Like(where.Field, value);
				case WhereOperand.Between: return repo.Between(where.Field, value);
				case WhereOperand.Since: return repo.Since(where.Field, value);
				case WhereOperand.Until: return repo.Until(where.Field, value);
				case WhereOperand.GreaterThan: return repo.GreaterThan(where.Field, value);
				case WhereOperand.GreaterThanOrEqual: return repo.GreaterThanOrEqual(where.Field, value);
				case WhereOperand.LessThan: return repo.LessThan(where.Field, value);
				case WhereOperand.LessThanOrEqual: return repo.LessThanOrEqual(where.Field, value);
			}

			throw new IndexOutOfRangeException(String.Format("Unhandled where condition type {0}", where.Type));
		}
	}
}
