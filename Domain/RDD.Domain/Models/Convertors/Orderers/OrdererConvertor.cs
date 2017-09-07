using RDD.Domain.Models.Convertors.Expressions;
using RDD.Domain.Models.Querying;
using RDD.Domain.Models.StorageQueries.Orderers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RDD.Domain.Models.Convertors.Orderers
{
	public class OrdererConvertor<T> : IOrdererConvertor<T> where T : class
	{
		IPropertyExpressionFactory _propertyExpressionFactory;

		public OrdererConvertor(IPropertyExpressionFactory propertyExpressionFactory)
		{
			_propertyExpressionFactory = propertyExpressionFactory;
		}

		public IOrderer<T> ConverterToOrderer(Query<T> request) => ConverterToOrderer(new Queue<OrderBy>(request.OrderBys));
		IOrderer<T> ConverterToOrderer(Queue<OrderBy> orderBys)
		{
			if (orderBys.Count == 0)
			{
				return new EmptyOrderer<T>();
			}
			else
			{
				var orderBy = orderBys.Dequeue();
				return OrderBy(orderBy.Field, orderBy.Direction, ConverterToOrderer(orderBys));
			}
		}

		public virtual IOrderer<T> OrderBy(string field, SortDirection sortDirection, IOrderer<T> next)
		{
			if (string.IsNullOrWhiteSpace(field))
			{
				throw new ArgumentException(nameof(field));
			}

			var type = typeof(T);
			var parameter = Expression.Parameter(type, "entity");
			var expression = _propertyExpressionFactory.GetMemberExpression(typeof(T), parameter, field, out var property);

			if (property.PropertyType == typeof(DateTime?))
			{
				return GetOrderyBy<DateTime?>(expression, parameter, sortDirection, next);
			}
			if (property.PropertyType == typeof(DateTime))
			{
				return GetOrderyBy<DateTime>(expression, parameter, sortDirection, next);
			}
			else if (property.PropertyType == typeof(int))
			{
				return GetOrderyBy<int>(expression, parameter, sortDirection, next);
			}
			else if (property.PropertyType.IsEnum)
			{
				return GetOrderyBy<int>(Expression.Convert(expression, typeof(int)), parameter, sortDirection, next);
			}
			else
			{
				return GetOrderyBy<object>(expression, parameter, sortDirection, next);
			}
		}

		protected IOrderer<T> GetOrderyBy<TKey>(Expression property, ParameterExpression param, SortDirection sortDirection, IOrderer<T> next)
		{
			return new Orderer<T, TKey>(Expression.Lambda<Func<T, TKey>>(property, param), sortDirection, next);
		}
	}
}