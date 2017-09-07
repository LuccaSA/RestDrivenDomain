using System;
using System.Linq.Expressions;
using System.Reflection;

namespace RDD.Domain.Models.Querying.Selectors.ExpressionSelectors
{
	public class PropertySelector<TSubject, TProperty> : ExpressionSelector<Expression<Func<TSubject, TProperty>>>
	{
		PropertyInfo _property;

		Expression<Func<TSubject, TProperty>> _expression;
		public override Expression<Func<TSubject, TProperty>> Expression => _expression;

		public PropertySelector(PropertyInfo property)
		{
			if (!property.DeclaringType.IsAssignableFrom(typeof(TSubject)))
				throw new ArgumentException("Property does not match TSubject type");

			if (property.PropertyType != typeof(TProperty))
				throw new ArgumentException("Property type does not match TProperty type");

			_property = property;

			var param = System.Linq.Expressions.Expression.Parameter(typeof(TSubject));
			var member = System.Linq.Expressions.Expression.Property(param, property);
			_expression = System.Linq.Expressions.Expression.Lambda<Func<TSubject, TProperty>>(member, param);
		}
	}
}