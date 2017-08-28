using RDD.Domain.Exceptions;
using RDD.Domain.Models.Querying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NExtends.Primitives.Types;
using System.Collections;
using LinqKit;
using RDD.Domain.Helpers;

namespace RDD.Domain.Models
{
	public partial class ReadOnlyRestCollection<TEntity, TKey> : IReadOnlyRestCollection<TEntity, TKey>
		where TEntity : class, IEntityBase<TEntity, TKey>
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
			else if (property.PropertyType.IsEnum)
			{
				return GetOrderyBy<int>(Expression.Convert(expression, typeof(int)), parameter, entities, direction, isFirst);
			}
			else
			{
				return GetOrderyBy<object>(expression, parameter, entities, direction, isFirst);
			}
		}

		protected IQueryable<TEntity> GetOrderyBy<TPropertyType>(Expression property, ParameterExpression param, IQueryable<TEntity> entities, SortDirection direction, bool isFirst)
		{
			var mySortExpression = Expression.Lambda<Func<TEntity, TPropertyType>>(property, param);

			return GetOrderyBy(entities, mySortExpression, direction, isFirst);
		}
		protected IQueryable<TEntity> GetOrderyBy<TPropertyType>(IQueryable<TEntity> entities, Expression<Func<TEntity, TPropertyType>> mySortExpression, SortDirection direction, bool isFirst)
		{
			if (isFirst)
			{
				return (direction == SortDirection.Descending) ? entities.OrderByDescending(mySortExpression)
					: entities.OrderBy(mySortExpression);
			}
			else
			{
				return (direction == SortDirection.Descending) ? ((IOrderedQueryable<TEntity>)entities).ThenByDescending(mySortExpression)
					: ((IOrderedQueryable<TEntity>)entities).ThenBy(mySortExpression);
				// var collectionParameter = Expression.Parameter(collectionType, "subparam");
				// var collectionEqualExpression = BuildEqualsRecursive(collectionParameter, subField, value);
				// Expression anyExpression = Expression.Lambda(typeof(Func<,>).MakeGenericType(collectionType, typeof(bool)), collectionEqualExpression, collectionParameter);

				// var accessCollectionExpression = new ExpressionHelper().NestedPropertyAccessor(parameter.Type, parameter, collectionAccessorField, out property);
				// return ExpressionHelper.BuildAny(collectionType, accessCollectionExpression, anyExpression);
				// }
				// else
				// {
				// return Expression.Equal(new ExpressionHelper().NestedPropertyAccessor(parameter.Type, parameter, field, out property), Expression.Constant(value, property.PropertyType));
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

				// Include internal properties through BindingFlags
				property = parentType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
					.Where(p => p.Name.ToLower() == member.ToLower())
					.FirstOrDefault();

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

		public Expression<Func<TEntity, bool>> OrFactory<TProp>(Func<TProp, Expression<Func<TEntity, bool>>> filter, IList values)
		{
			var expression = PredicateBuilder.False<TEntity>();
			foreach (TProp val in values)
			{
				expression = expression.Or(filter(val)).Expand();
			}
			return expression.Expand();
		}

		public Expression<Func<TEntity, bool>> AndFactory<TProp>(Func<TProp, Expression<Func<TEntity, bool>>> filter, IList values)
		{
			var expression = PredicateBuilder.True<TEntity>();
			foreach (TProp val in values)
			{
				expression = expression.And(filter(val)).Expand();
			}
			return expression.Expand();
		}

		public virtual Expression<Func<TEntity, bool>> Equals(string field, IList values)
		{
			return BuildBinaryExpression(WhereOperand.Equals, field, values);
		}

		public virtual Expression<Func<TEntity, bool>> NotEqual(string field, IList values)
		{
			return AndFactory<object>(value => BuildBinaryExpression(WhereOperand.NotEqual, field, value), values);
		}

		public Expression<Func<TEntity, bool>> Until(string field, IList values)
		{
			return OrFactory<object>(value => Until(field, value), values);
		}
		protected virtual Expression<Func<TEntity, bool>> Until(string field, object value)
		{
			return BuildBinaryExpression(WhereOperand.Until, field, value);
		}

		public Expression<Func<TEntity, bool>> Since(string field, IList values)
		{
			return OrFactory<object>(value => Since(field, value), values);
		}
		protected virtual Expression<Func<TEntity, bool>> Since(string field, object value)
		{
			return BuildBinaryExpression(WhereOperand.Since, field, value);
		}

		public Expression<Func<TEntity, bool>> GreaterThan(string field, IList values)
		{
			return OrFactory<object>(value => GreaterThan(field, value), values);
		}
		protected virtual Expression<Func<TEntity, bool>> GreaterThan(string field, object value)
		{
			return BuildBinaryExpression(WhereOperand.GreaterThan, field, value);
		}

		public Expression<Func<TEntity, bool>> GreaterThanOrEqual(string field, IList values)
		{
			return OrFactory<object>(value => GreaterThanOrEqual(field, value), values);
		}
		protected virtual Expression<Func<TEntity, bool>> GreaterThanOrEqual(string field, object value)
		{
			return BuildBinaryExpression(WhereOperand.GreaterThanOrEqual, field, value);
		}

		public Expression<Func<TEntity, bool>> LessThan(string field, IList values)
		{
			return OrFactory<object>(value => LessThan(field, value), values);
		}
		protected virtual Expression<Func<TEntity, bool>> LessThan(string field, object value)
		{
			return BuildBinaryExpression(WhereOperand.LessThan, field, value);
		}

		public Expression<Func<TEntity, bool>> LessThanOrEqual(string field, IList values)
		{
			return OrFactory<object>(value => LessThanOrEqual(field, value), values);
		}
		protected virtual Expression<Func<TEntity, bool>> LessThanOrEqual(string field, object value)
		{
			return BuildBinaryExpression(WhereOperand.LessThanOrEqual, field, value);
		}

		public Expression<Func<TEntity, bool>> Between(string field, IList values)
		{
			return OrFactory<object>(value => Between(field, value), values);
		}
		protected virtual Expression<Func<TEntity, bool>> Between(string field, object value)
		{
			return BuildBinaryExpression(WhereOperand.Between, field, value);
		}

		public Expression<Func<TEntity, bool>> Starts(string field, IList values)
		{
			return OrFactory<string>(value => Starts(field, value), values);
		}
		protected virtual Expression<Func<TEntity, bool>> Starts(string field, string value)
		{
			var parameter = Expression.Parameter(typeof(TEntity), "entity");
			var expression = NestedPropertyAccessor(parameter, field);
			var comparisonMethod = typeof(string).GetMethod("StartsWith", new Type[] { typeof(string) });
			var toLowerMethod = typeof(string).GetMethod("ToLower", new Type[] { });

			var startsWithExpression = Expression.Call(Expression.Call(expression, toLowerMethod), comparisonMethod, Expression.Constant(value.ToLower(), typeof(string)));

			return Expression.Lambda<Func<TEntity, bool>>(startsWithExpression, parameter);
		}

		public Expression<Func<TEntity, bool>> Like(string field, IList values)
		{
			return OrFactory<string>(value => Like(field, value), values);
		}
		protected virtual Expression<Func<TEntity, bool>> Like(string field, string value)
		{
			var parameter = Expression.Parameter(typeof(TEntity), "entity");
			var expression = NestedPropertyAccessor(parameter, field);
			var comparisonMethod = typeof(string).GetMethod("Contains", new Type[] { typeof(string) });
			var toLowerMethod = typeof(string).GetMethod("ToLower", new Type[] { });

			var containsExpression = Expression.Call(Expression.Call(expression, toLowerMethod), comparisonMethod, Expression.Constant(value.ToLower(), typeof(string)));

			return Expression.Lambda<Func<TEntity, bool>>(containsExpression, parameter);
		}

		private Expression<Func<TEntity, bool>> BuildBinaryExpression(WhereOperand binaryOperator, string field, object value)
		{
			var type = typeof(TEntity);
			var parameter = Expression.Parameter(type, "entity");
			PropertyInfo property;
			var expression = BuildBinaryExpressionRecursive(binaryOperator, parameter, field, value, out property);

			// Limitation à certains types
			if (binaryOperator == WhereOperand.Until || binaryOperator == WhereOperand.Since)
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

		private Expression BuildBinaryExpressionRecursive(WhereOperand binaryOperator, ParameterExpression parameter, string field, object value, out PropertyInfo property)
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
				return ExpressionHelper.BuildAny(collectionType, accessCollectionExpression, anyExpression);
			}
			else
			{
				Expression expressionLeft = NestedPropertyAccessor(parameter.Type, parameter, field, out property);

				// Hack pour le Between qui n'est pas binaire, mais plus performant de le faire ici plutot que 2 parcours récursifs, puis un AND sur les expressions
				if (binaryOperator == WhereOperand.Between)
				{
					var period = (Period)value;
					ConstantExpression expressionRightSince = (value == null) ? Expression.Constant(null) : Expression.Constant(period.Start, property.PropertyType);
					ConstantExpression expressionRightUntil = (value == null) ? Expression.Constant(null) : Expression.Constant(period.End, property.PropertyType);
					var sinceExpression = Expression.GreaterThanOrEqual(expressionLeft, expressionRightSince);
					var untilExpression = Expression.LessThanOrEqual(expressionLeft, expressionRightUntil);
					return Expression.AndAlso(sinceExpression, untilExpression);
				}

				ConstantExpression expressionRight;
				switch (binaryOperator)
				{
					case WhereOperand.Equals:
						expressionRight = Expression.Constant(value);
						break;
					default:
						//précision du type nécessaie pour les nullables
						expressionRight = Expression.Constant(value, expressionLeft.Type);
						break;
				}

				switch (binaryOperator)
				{
					case WhereOperand.Equals: return Expression.Call(typeof(Enumerable), "Contains", new Type[] { expressionLeft.Type }, expressionRight, expressionLeft);
					case WhereOperand.NotEqual: return Expression.NotEqual(expressionLeft, expressionRight);
					case WhereOperand.GreaterThan: return Expression.GreaterThan(expressionLeft, expressionRight);
					case WhereOperand.LessThan: return Expression.LessThan(expressionLeft, expressionRight);

					case WhereOperand.Since:
					case WhereOperand.GreaterThanOrEqual:
						if (value == null)
							return Expression.Equal(expressionLeft, expressionRight);
						else
							return Expression.GreaterThanOrEqual(expressionLeft, expressionRight);

					case WhereOperand.Until:
					case WhereOperand.LessThanOrEqual:
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
				// Include internal properties through BindingFlags
				property = type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
					.Where(p => p.Name.ToLower() == member.ToLower())
					.FirstOrDefault();

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