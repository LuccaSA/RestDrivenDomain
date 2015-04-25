using RDD.Infra;
using RDD.Infra.Helpers;
using RDD.Infra.Models.Exceptions;
using RDD.Infra.Models.Querying;
using RDD.Infra.Models.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain.Services
{
	public partial class RestDomainService<TEntity, TKey> : IRestDomainService<TEntity, TKey>
		where TEntity : class, IEntityBase<TKey>
		where TKey : IEquatable<TKey>
	{
		public virtual IQueryable<TEntity> OrderByDefault(IQueryable<TEntity> entities)
		{
			return entities.OrderBy(e => e.Id);
		}
		public virtual IQueryable<TEntity> OrderBy(IQueryable<TEntity> entities, string field, SortDirection direction, bool isFirst = true)
		{
			if (string.IsNullOrWhiteSpace(field)) { return entities; }

			var type = typeof(TEntity);
			var parameter = Expression.Parameter(type, "entity");
			PropertyInfo property;
			var expression = NestedPropertyAccessor(parameter, field, out property);

			if (property.PropertyType == typeof(DateTime?))
			{
				return GetOrderyBy<DateTime?>(expression, parameter, entities, direction, isFirst);
			}
			if (property.PropertyType == typeof(DateTime))
			{
				return GetOrderyBy<DateTime>(expression, parameter, entities, direction, isFirst);
			}
			else if (property.PropertyType == typeof(int))
			{
				return GetOrderyBy<int>(expression, parameter, entities, direction, isFirst);
			}
			else
			{
				return GetOrderyBy<object>(expression, parameter, entities, direction, isFirst);
			}
		}

		private IQueryable<TEntity> GetOrderyBy<TPropertyType>(Expression property, ParameterExpression param, IQueryable<TEntity> entities, SortDirection direction, bool isFirst)
		{
			var mySortExpression = Expression.Lambda<Func<TEntity, TPropertyType>>(property, param);
			if (isFirst)
			{
				return (direction == SortDirection.Descending) ? entities.Select(e => (TEntity)e).OrderByDescending(mySortExpression)
					: entities.Select(e => (TEntity)e).OrderBy(mySortExpression);
			}
			else
			{
				return (direction == SortDirection.Descending) ? ((IOrderedQueryable<TEntity>)entities.Select(e => (TEntity)e)).ThenByDescending(mySortExpression)
					: ((IOrderedQueryable<TEntity>)entities.Select(e => (TEntity)e)).ThenBy(mySortExpression);
			}
		}

		private bool IsQueryOnCollection(string field, out string collectionAccessorField, out string subField, out Type collectionType)
		{
			return IsQueryOnCollection(typeof(TEntity), field, out collectionAccessorField, out subField, out collectionType);
		}
		private bool IsQueryOnCollection(Type parentType, string field, out string collectionAccessorField, out string subField, out Type collectionType)
		{
			PropertyInfo property = null;
			collectionAccessorField = "";
			subField = field;
			collectionType = typeof(object);
			var fields = field.Split('.');
			for (int i = 0; i < fields.Length; i++)
			{
				var member = fields[i];

				property = parentType.GetProperties().Where(p => p.Name.ToLower() == member.ToLower()).FirstOrDefault();

				if (property == null)
				{
					throw new HttpLikeException(System.Net.HttpStatusCode.BadRequest, String.Format("Unknown property {0} on type {1}", member, parentType.Name));
				}

				if (!property.CanRead)
				{
					throw new HttpLikeException(System.Net.HttpStatusCode.BadRequest, String.Format("Property {0} of type {1} is set only", member, parentType.Name));
				}


				if (property.PropertyType.IsEnumerableOrArray())
				{
					collectionAccessorField = string.Join(".", fields.Take(i + 1).ToArray());
					subField = string.Join(".", fields.Skip(i + 1).ToArray());
					collectionType = property.PropertyType.GetEnumerableOrArrayElementType();
					return true;
				}

				parentType = property.PropertyType;
			}

			return false;
		}

		public virtual Expression<Func<TEntity, bool>> Equals(string field, object value)
		{
			return BuildBinaryExpression(FilterOperand.Equals, field, value);
		}
		public virtual Expression<Func<TEntity, bool>> NotEqual(string field, object value)
		{
			return BuildBinaryExpression(FilterOperand.NotEqual, field, value);
		}
		public virtual Expression<Func<TEntity, bool>> Until(string field, object value)
		{
			return BuildBinaryExpression(FilterOperand.Until, field, value);
		}
		public virtual Expression<Func<TEntity, bool>> Since(string field, object value)
		{
			return BuildBinaryExpression(FilterOperand.Since, field, value);
		}
		public virtual Expression<Func<TEntity, bool>> GreaterThan(string field, object value)
		{
			return BuildBinaryExpression(FilterOperand.GreaterThan, field, value);
		}
		public virtual Expression<Func<TEntity, bool>> GreaterThanOrEqual(string field, object value)
		{
			return BuildBinaryExpression(FilterOperand.GreaterThanOrEqual, field, value);
		}
		public virtual Expression<Func<TEntity, bool>> LessThan(string field, object value)
		{
			return BuildBinaryExpression(FilterOperand.LessThan, field, value);
		}
		public virtual Expression<Func<TEntity, bool>> LessThanOrEqual(string field, object value)
		{
			return BuildBinaryExpression(FilterOperand.LessThanOrEqual, field, value);
		}
		public virtual Expression<Func<TEntity, bool>> Between(string field, object value)
		{
			return BuildBinaryExpression(FilterOperand.Between, field, value);
		}

		public virtual Expression<Func<TEntity, bool>> Starts(string field, object value)
		{
			var type = typeof(TEntity);

			var parameter = Expression.Parameter(type, "entity");

			var expression = NestedPropertyAccessor(parameter, field);

			var comparisonMethod = typeof(string).GetMethod("IndexOf", new Type[] { typeof(string), typeof(StringComparison) });

			var containsExpression = Expression.Equal(Expression.Call(expression, comparisonMethod, Expression.Constant(value, typeof(string)), Expression.Constant(StringComparison.InvariantCultureIgnoreCase, typeof(StringComparison))), Expression.Constant(0, typeof(int)));

			return Expression.Lambda<Func<TEntity, bool>>(containsExpression, parameter);
		}
		public virtual Expression<Func<TEntity, bool>> Like(string field, object value)
		{
			var type = typeof(TEntity);

			var parameter = Expression.Parameter(type, "entity");

			var expression = NestedPropertyAccessor(parameter, field);

			var comparisonMethod = typeof(string).GetMethod("Contains", new Type[] { typeof(string) });
			var toLowerMethod = typeof(string).GetMethod("ToLower", new Type[] { });

			var containsExpression = Expression.Call(Expression.Call(expression, toLowerMethod), comparisonMethod, Expression.Constant(value.ToString().ToLower(), typeof(string)));

			return Expression.Lambda<Func<TEntity, bool>>(containsExpression, parameter);
		}

		private Expression<Func<TEntity, bool>> BuildBinaryExpression(FilterOperand binaryOperator, string field, object value)
		{
			var type = typeof(TEntity);
			var parameter = Expression.Parameter(type, "entity");
			PropertyInfo property;
			var expression = BuildBinaryExpressionRecursive(binaryOperator, parameter, field, value, out property);

			// Limitation à certains types
			if (binaryOperator == FilterOperand.Until || binaryOperator == FilterOperand.Since)
			{
				var propertyReturnType = property.GetGetMethod().ReturnType;
				if (propertyReturnType.IsGenericType)
				{
					propertyReturnType = propertyReturnType.GenericTypeArguments[0];
				}
				if (propertyReturnType != typeof(DateTime))
				{
					throw new HttpLikeException(System.Net.HttpStatusCode.BadRequest, String.Format("Operator '{2}' only allows dates to be compared, whereas property {0} is of type {1}.", field, property.GetType().Name, binaryOperator));
				}
			}

			return Expression.Lambda<Func<TEntity, bool>>(expression, parameter);
		}
		private Expression BuildBinaryExpressionRecursive(FilterOperand binaryOperator, ParameterExpression parameter, string field, object value, out PropertyInfo property)
		{
			string collectionAccessorField;
			string subField;
			Type collectionType;

			if (this.IsQueryOnCollection(parameter.Type, field, out collectionAccessorField, out subField, out collectionType))
			{
				var collectionParameter = Expression.Parameter(collectionType, "subparam");
				var collectionBinaryExpression = BuildBinaryExpressionRecursive(binaryOperator, collectionParameter, subField, value, out property);

				Expression anyExpression = Expression.Lambda(typeof(Func<,>).MakeGenericType(collectionType, typeof(bool)), collectionBinaryExpression, collectionParameter);

				PropertyInfo accessProperty;
				var accessCollectionExpression = NestedPropertyAccessor(parameter.Type, parameter, collectionAccessorField, out accessProperty);
				return ExpressionManipulationHelper.BuildAny(collectionType, accessCollectionExpression, anyExpression);
			}
			else
			{
				Expression expressionLeft = NestedPropertyAccessor(parameter.Type, parameter, field, out property);

				// Hack pour le Between qui n'est pas binaire, mais plus performant de le faire ici plutot que 2 parcours récursifs, puis un AND sur les expressions
				if (binaryOperator == FilterOperand.Between)
				{
					var period = (Period)value;
					ConstantExpression expressionRightSince = (value == null) ? Expression.Constant(null) : Expression.Constant(period.Start, property.PropertyType);
					ConstantExpression expressionRightUntil = (value == null) ? Expression.Constant(null) : Expression.Constant(period.End, property.PropertyType);
					var sinceExpression = Expression.GreaterThanOrEqual(expressionLeft, expressionRightSince);
					var untilExpression = Expression.LessThanOrEqual(expressionLeft, expressionRightUntil);
					return Expression.AndAlso(sinceExpression, untilExpression);
				}

				ConstantExpression expressionRight = (value == null) ? Expression.Constant(null) : Expression.Constant(value, property.PropertyType);

				switch (binaryOperator)
				{
					case FilterOperand.Equals:
						return Expression.Equal(expressionLeft, expressionRight);

					case FilterOperand.NotEqual:
						return Expression.NotEqual(expressionLeft, expressionRight);

					case FilterOperand.GreaterThan:
						return Expression.GreaterThan(expressionLeft, expressionRight);

					case FilterOperand.Since:
					case FilterOperand.GreaterThanOrEqual:
						if (value == null)
							return Expression.Equal(expressionLeft, expressionRight);
						else
							return Expression.GreaterThanOrEqual(expressionLeft, expressionRight);

					case FilterOperand.LessThan:
						return Expression.LessThan(expressionLeft, expressionRight);

					case FilterOperand.Until:
					case FilterOperand.LessThanOrEqual:
						if (value == null)
							return Expression.Equal(expressionLeft, expressionRight);
						else
							return Expression.LessThanOrEqual(expressionLeft, expressionRight);

					default:
						throw new NotImplementedException(string.Format("L'expression binaire n'est pas gérée pour l'opérateur fourni: '{0}'.", binaryOperator));
				}
			}
		}

		private Expression NestedPropertyAccessor(ParameterExpression seed, string field)
		{
			PropertyInfo unusedFinalProperty;
			return NestedPropertyAccessor(seed, field, out unusedFinalProperty);
		}
		private Expression NestedPropertyAccessor(ParameterExpression seed, string field, out PropertyInfo property)
		{
			var type = typeof(TEntity); //Ici on triche un peu en utilisant cette variable, car on est censé passer au moins 1 fois dans le foreach pour setter finalPropertyType proprement
			return NestedPropertyAccessor(type, seed, field, out property);
		}
		private Expression NestedPropertyAccessor(Type type, ParameterExpression seed, string field, out PropertyInfo property)
		{
			return NestedPropertyAccessor(type, seed, field.Split('.'), out property);
		}
		private Expression NestedPropertyAccessor(Type type, ParameterExpression seed, string[] fields, out PropertyInfo property)
		{
			property = null;
			Expression body = seed;

			foreach (var member in fields)
			{
				property = type.GetProperties().Where(p => p.Name.ToLower() == member.ToLower()).FirstOrDefault();

				if (property == null)
				{
					throw new HttpLikeException(System.Net.HttpStatusCode.BadRequest, String.Format("Unknown property {0} on type {1}", member, type.Name));
				}

				if (!property.CanRead)
				{
					throw new HttpLikeException(System.Net.HttpStatusCode.BadRequest, String.Format("Property {0} of type {1} is set only", member, type.Name));
				}

				body = Expression.PropertyOrField(body, member);

				type = property.PropertyType;
			}

			return body;
		}
	}
}
