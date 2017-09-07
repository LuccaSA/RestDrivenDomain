using LinqKit;
using NExtends.Primitives.Types;
using RDD.Domain.Exceptions;
using RDD.Domain.Helpers;
using RDD.Domain.Models;
using RDD.Domain.Models.Querying;
using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace RDD.Domain.Models.Convertors.Expressions
{
	class ExpressionGenerator<T> : IExpressionGenerator<T> where T : class
	{
		IPropertyExpressionFactory _propertyExpressionFactory;

		public ExpressionGenerator(IPropertyExpressionFactory propertyExpressionFactory)
		{
			_propertyExpressionFactory = propertyExpressionFactory;
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

		public Expression<Func<T, bool>> OrFactory<TProp>(Func<TProp, Expression<Func<T, bool>>> filter, IList values)
		{
			var expression = PredicateBuilder.False<T>();
			foreach (TProp val in values)
			{
				expression = expression.Or(filter(val)).Expand();
			}
			return expression.Expand();
		}

		public Expression<Func<T, bool>> AndFactory<TProp>(Func<TProp, Expression<Func<T, bool>>> filter, IList values)
		{
			var expression = PredicateBuilder.True<T>();
			foreach (TProp val in values)
			{
				expression = expression.And(filter(val)).Expand();
			}
			return expression.Expand();
		}

		public virtual Expression<Func<T, bool>> Equals(string field, IList values)
		{
			return BuildBinaryExpression(WhereOperand.Equals, field, values);
		}

		public virtual Expression<Func<T, bool>> NotEqual(string field, IList values)
		{
			return AndFactory<object>(value => BuildBinaryExpression(WhereOperand.NotEqual, field, value), values);
		}

		public Expression<Func<T, bool>> Until(string field, IList values)
		{
			return OrFactory<object>(value => Until(field, value), values);
		}
		protected virtual Expression<Func<T, bool>> Until(string field, object value)
		{
			return BuildBinaryExpression(WhereOperand.Until, field, value);
		}

		public Expression<Func<T, bool>> Since(string field, IList values)
		{
			return OrFactory<object>(value => Since(field, value), values);
		}
		protected virtual Expression<Func<T, bool>> Since(string field, object value)
		{
			return BuildBinaryExpression(WhereOperand.Since, field, value);
		}

		public Expression<Func<T, bool>> GreaterThan(string field, IList values)
		{
			return OrFactory<object>(value => GreaterThan(field, value), values);
		}
		protected virtual Expression<Func<T, bool>> GreaterThan(string field, object value)
		{
			return BuildBinaryExpression(WhereOperand.GreaterThan, field, value);
		}

		public Expression<Func<T, bool>> GreaterThanOrEqual(string field, IList values)
		{
			return OrFactory<object>(value => GreaterThanOrEqual(field, value), values);
		}
		protected virtual Expression<Func<T, bool>> GreaterThanOrEqual(string field, object value)
		{
			return BuildBinaryExpression(WhereOperand.GreaterThanOrEqual, field, value);
		}

		public Expression<Func<T, bool>> LessThan(string field, IList values)
		{
			return OrFactory<object>(value => LessThan(field, value), values);
		}
		protected virtual Expression<Func<T, bool>> LessThan(string field, object value)
		{
			return BuildBinaryExpression(WhereOperand.LessThan, field, value);
		}

		public Expression<Func<T, bool>> LessThanOrEqual(string field, IList values)
		{
			return OrFactory<object>(value => LessThanOrEqual(field, value), values);
		}
		protected virtual Expression<Func<T, bool>> LessThanOrEqual(string field, object value)
		{
			return BuildBinaryExpression(WhereOperand.LessThanOrEqual, field, value);
		}

		public Expression<Func<T, bool>> Between(string field, IList values)
		{
			return OrFactory<object>(value => Between(field, value), values);
		}
		protected virtual Expression<Func<T, bool>> Between(string field, object value)
		{
			return BuildBinaryExpression(WhereOperand.Between, field, value);
		}

		public Expression<Func<T, bool>> Starts(string field, IList values)
		{
			return OrFactory<string>(value => Starts(field, value), values);
		}
		protected virtual Expression<Func<T, bool>> Starts(string field, string value)
		{
			var parameter = Expression.Parameter(typeof(T), "entity");
			var expression = _propertyExpressionFactory.GetMemberExpression(typeof(T), parameter, field, out var property);
			var comparisonMethod = typeof(string).GetMethod("StartsWith", new Type[] { typeof(string) });
			var toLowerMethod = typeof(string).GetMethod("ToLower", new Type[] { });

			var startsWithExpression = Expression.Call(Expression.Call(expression, toLowerMethod), comparisonMethod, Expression.Constant(value.ToLower(), typeof(string)));

			return Expression.Lambda<Func<T, bool>>(startsWithExpression, parameter);
		}

		public Expression<Func<T, bool>> Like(string field, IList values)
		{
			return OrFactory<string>(value => Like(field, value), values);
		}
		protected virtual Expression<Func<T, bool>> Like(string field, string value)
		{
			var parameter = Expression.Parameter(typeof(T), "entity");
			var expression = _propertyExpressionFactory.GetMemberExpression(typeof(T), parameter, field, out var property);
			var comparisonMethod = typeof(string).GetMethod("Contains", new Type[] { typeof(string) });
			var toLowerMethod = typeof(string).GetMethod("ToLower", new Type[] { });

			var containsExpression = Expression.Call(Expression.Call(expression, toLowerMethod), comparisonMethod, Expression.Constant(value.ToLower(), typeof(string)));

			return Expression.Lambda<Func<T, bool>>(containsExpression, parameter);
		}

		private Expression<Func<T, bool>> BuildBinaryExpression(WhereOperand binaryOperator, string field, object value)
		{
			var type = typeof(T);
			var parameter = Expression.Parameter(type, "entity");
			var expression = BuildBinaryExpressionRecursive(binaryOperator, parameter, field, value, out var property);

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

			return Expression.Lambda<Func<T, bool>>(expression, parameter);
		}

		private Expression BuildBinaryExpressionRecursive(WhereOperand binaryOperator, ParameterExpression parameter, string field, object value, out PropertyInfo property)
		{
			string collectionAccessorField;
			string subField;
			Type collectionType;

			if (IsQueryOnCollection(parameter.Type, field, out collectionAccessorField, out subField, out collectionType))
			{
				var collectionParameter = Expression.Parameter(collectionType, "subparam");
				var collectionBinaryExpression = BuildBinaryExpressionRecursive(binaryOperator, collectionParameter, subField, value, out property);

				Expression anyExpression = Expression.Lambda(typeof(Func<,>).MakeGenericType(collectionType, typeof(bool)), collectionBinaryExpression, collectionParameter);

				PropertyInfo accessProperty;
				var accessCollectionExpression = _propertyExpressionFactory.GetMemberExpression(parameter.Type, parameter, collectionAccessorField, out accessProperty);
				return ExpressionHelper.BuildAny(collectionType, accessCollectionExpression, anyExpression);
			}
			else
			{
				var expressionLeft = _propertyExpressionFactory.GetMemberExpression(parameter.Type, parameter, field, out property);

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
	}
}