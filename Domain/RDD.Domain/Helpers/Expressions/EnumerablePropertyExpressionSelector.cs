using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RDD.Domain.Helpers.Expressions
{
    public class EnumerablePropertyExpressionSelector : PropertyExpressionSelector
    {
        public override Type ResultType => Property.PropertyType.GenericTypeArguments[0];

        public static EnumerablePropertyExpressionSelector New<TClass, TProp>(Expression<Func<TClass, IEnumerable<TProp>>> lambda)
        {
            return new EnumerablePropertyExpressionSelector { LambdaExpression = lambda };
        }
    }
}
