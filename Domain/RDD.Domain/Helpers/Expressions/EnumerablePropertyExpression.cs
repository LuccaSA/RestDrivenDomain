using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RDD.Domain.Helpers.Expressions
{
    public class EnumerablePropertyExpression : PropertyExpression
    {
        public override Type ResultType => Property.PropertyType.GenericTypeArguments[0];

        public static EnumerablePropertyExpression New<TClass, TProp>(Expression<Func<TClass, IEnumerable<TProp>>> lambda)
        {
            return new EnumerablePropertyExpression { LambdaExpression = lambda };
        }
    }
}
