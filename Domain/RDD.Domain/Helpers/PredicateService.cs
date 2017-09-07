using LinqKit;
using NExtends.Primitives.Types;
using RDD.Domain.Exceptions;
using RDD.Domain.Models;
using RDD.Domain.Models.Querying;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain.Helpers
{
	internal class PredicateService<TEntity>
		where TEntity : class, IEntityBase
	{
		internal Expression<Func<TEntity, bool>> BuildBinaryExpression(FilterOperand binaryOperator, string field, object value)
		{
			var entityType = typeof(TEntity);
			var parameter = Expression.Parameter(entityType, "entity");
			PropertyInfo property;
			var expression = BuildBinaryExpressionRecursive(entityType, binaryOperator, parameter, field, value, out property);

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

		private Expression BuildBinaryExpressionRecursive(Type entityType, FilterOperand binaryOperator, ParameterExpression parameter, string field, object value, out PropertyInfo property)
		{
			string collectionAccessorField;
			string subField;
			Type collectionType;

			if (IsQueryOnCollection(entityType, field, out collectionAccessorField, out subField, out collectionType))
			{
				var collectionParameter = Expression.Parameter(collectionType, "subparam");
				var collectionBinaryExpression = BuildBinaryExpressionRecursive(entityType, binaryOperator, collectionParameter, subField, value, out property);

				Expression anyExpression = Expression.Lambda(typeof(Func<,>).MakeGenericType(collectionType, typeof(bool)), collectionBinaryExpression, collectionParameter);

				PropertyInfo accessProperty;
				var accessCollectionExpression = NestedPropertyAccessor(parameter.Type, parameter, collectionAccessorField, out accessProperty);
				return ExpressionHelper.BuildAny(collectionType, accessCollectionExpression, anyExpression);
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

				ConstantExpression expressionRight;
				switch (binaryOperator)
				{
					case FilterOperand.Equals:
						expressionRight = Expression.Constant(value);
						break;
					default:
						//précision du type nécessaie pour les nullables
						expressionRight = Expression.Constant(value, expressionLeft.Type);
						break;
				}

				switch (binaryOperator)
				{
					case FilterOperand.Equals: return Expression.Call(typeof(Enumerable), "Contains", new Type[] { expressionLeft.Type }, expressionRight, expressionLeft);
					case FilterOperand.NotEqual: return Expression.NotEqual(expressionLeft, expressionRight);
					case FilterOperand.GreaterThan: return Expression.GreaterThan(expressionLeft, expressionRight);
					case FilterOperand.LessThan: return Expression.LessThan(expressionLeft, expressionRight);

					case FilterOperand.Since:
					case FilterOperand.GreaterThanOrEqual:
						if (value == null)
							return Expression.Equal(expressionLeft, expressionRight);
						else
							return Expression.GreaterThanOrEqual(expressionLeft, expressionRight);

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
		internal virtual Expression<Func<TEntity, bool>> BuildStartsExpression(string field, string value)
		{
			var entityType = typeof(TEntity);
			var parameter = Expression.Parameter(entityType, "entity");
			var expression = NestedPropertyAccessor(entityType, parameter, field);
			var comparisonMethod = typeof(string).GetMethod("StartsWith", new Type[] { typeof(string) });
			var toLowerMethod = typeof(string).GetMethod("ToLower", new Type[] { });

			var startsWithExpression = Expression.Call(Expression.Call(expression, toLowerMethod), comparisonMethod, Expression.Constant(value.ToLower(), typeof(string)));

			return Expression.Lambda<Func<TEntity, bool>>(startsWithExpression, parameter);
		}
		internal virtual Expression<Func<TEntity, bool>> BuildLikeExpression(string field, string value)
		{
			var entityType = typeof(TEntity);
			var parameter = Expression.Parameter(entityType, "entity");
			var expression = NestedPropertyAccessor(entityType, parameter, field);
			var comparisonMethod = typeof(string).GetMethod("Contains", new Type[] { typeof(string) });
			var toLowerMethod = typeof(string).GetMethod("ToLower", new Type[] { });

			var containsExpression = Expression.Call(Expression.Call(expression, toLowerMethod), comparisonMethod, Expression.Constant(value.ToLower(), typeof(string)));

			return Expression.Lambda<Func<TEntity, bool>>(containsExpression, parameter);
		}
		private Expression NestedPropertyAccessor(Type type, ParameterExpression seed, string field)
		{
			PropertyInfo unusedFinalProperty;
			return NestedPropertyAccessor(type, seed, field, out unusedFinalProperty);
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
		internal Expression<Func<TEntity, bool>> AndFactory<TEntity, TProp>(Func<TProp, Expression<Func<TEntity, bool>>> filter, IList values)
		{
			var expression = PredicateBuilder.True<TEntity>();
			foreach (TProp val in values)
			{
				expression = expression.And(filter(val)).Expand();
			}
			return expression.Expand();
		}
		internal Expression<Func<TEntity, bool>> OrFactory<TEntity, TProp>(Func<TProp, Expression<Func<TEntity, bool>>> filter, IList values)
		{
			var expression = PredicateBuilder.False<TEntity>();
			foreach (TProp val in values)
			{
				expression = expression.Or(filter(val)).Expand();
			}
			return expression.Expand();
		}

		private bool IsQueryOnCollection(Type entityType, string field, out string collectionAccessorField, out string subField, out Type collectionType)
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
				property = entityType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
					.Where(p => p.Name.ToLower() == member.ToLower())
					.FirstOrDefault();

				if (property == null)
				{
					throw new HttpLikeException(System.Net.HttpStatusCode.BadRequest, String.Format("Unknown property {0} on type {1}", member, entityType.Name));
				}

				if (!property.CanRead)
				{
					throw new HttpLikeException(System.Net.HttpStatusCode.BadRequest, String.Format("Property {0} of type {1} is set only", member, entityType.Name));
				}


				if (property.PropertyType.IsEnumerableOrArray())
				{
					collectionAccessorField = string.Join(".", fields.Take(i + 1).ToArray());
					subField = string.Join(".", fields.Skip(i + 1).ToArray());
					collectionType = property.PropertyType.GetEnumerableOrArrayElementType();
					return true;
				}

				entityType = property.PropertyType;
			}

			return false;
		}
	}
}
