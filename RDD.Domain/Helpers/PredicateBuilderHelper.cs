using LinqKit;
using RDD.Infra;
using RDD.Infra.Models.Querying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain.Helpers
{
	public class PredicateBuilderHelper
	{
		private ICollection<Filter> _filters;

		public PredicateBuilderHelper(ICollection<Filter> filters)
		{
			_filters = filters;
		}

		public Expression<Func<TObject, bool>> GetPredicate<TObject>()
			where TObject : class
		{
			var feed = PredicateBuilder.True<TObject>();

			for (int i = 0, length = _filters.Count; i < length; i++)
			{
				var filter = _filters.ElementAt(i);
				var sub = PredicateBuilder.False<TObject>();

				foreach (var value in filter.Values)
				{
					sub = sub.Or(ToExpression<TObject>(filter, value).Expand());
				}

				feed = feed.And(sub.Expand());
			}

			return feed.Expand();
		}

		private Expression<Func<TObject, bool>> ToExpression<TObject>(Filter filter, object value)
			where TObject : class
		{
			var filterField = filter.Field;
			var filterType = filter.Type;

			switch (filterType)
			{
				case FilterOperand.Equals:

					var type = typeof(TObject);
					var property = type.GetProperties().Where(p => p.Name.ToLower() == filterField.ToLower()).FirstOrDefault();

					var parameter = Expression.Parameter(type, "entity");

					var body = Expression.PropertyOrField(parameter, filterField);

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
		public Expression<Func<TEntity, bool>> GetEntityPredicate<TEntity, TKey>(IRestDomainService<TEntity, TKey> service)
			where TEntity : class, IEntityBase<TKey>
			where TKey : IEquatable<TKey>
		{
			var feed = PredicateBuilder.True<TEntity>();

			for (int i = 0, length = _filters.Count; i < length; i++)
			{
				var filter = _filters.ElementAt(i);
				var sub = PredicateBuilder.False<TEntity>();

				foreach (var value in filter.Values)
				{
					sub = sub.Or(ToEntityExpression(service, filter, value).Expand());
				}

				feed = feed.And(sub.Expand());
			}

			return feed.Expand();
		}

		private Expression<Func<TEntity, bool>> ToEntityExpression<TEntity, TKey>(IRestDomainService<TEntity, TKey> service, Filter filter, object value)
			where TEntity : class, IEntityBase<TKey>
			where TKey : IEquatable<TKey>
		{
			var field = filter.Field;
			var type = filter.Type;

			switch (type)
			{
				case FilterOperand.Equals:
					return service.Equals(field, value);
				case FilterOperand.NotEqual:
					return service.NotEqual(field, value);
				case FilterOperand.Starts:
					return service.Starts(field, value);
				case FilterOperand.Like:
					return service.Like(field, value);
				case FilterOperand.Between:
					return service.Between(field, value);
				case FilterOperand.Since:
					return service.Since(field, value);
				case FilterOperand.Until:
					return service.Until(field, value);
				case FilterOperand.GreaterThan:
					return service.GreaterThan(field, value);
				case FilterOperand.GreaterThanOrEqual:
					return service.GreaterThanOrEqual(field, value);
				case FilterOperand.LessThan:
					return service.LessThan(field, value);
				case FilterOperand.LessThanOrEqual:
					return service.LessThanOrEqual(field, value);
			}

			throw new IndexOutOfRangeException(String.Format("Unhandled where condition type {0}", type));
		}
	}
}
