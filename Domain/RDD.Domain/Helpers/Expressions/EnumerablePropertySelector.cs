using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RDD.Domain.Helpers.Expressions
{
    public class EnumerablePropertySelector : SimplePropertySelector
    {
        public override Type ResultType => Property.PropertyType.GenericTypeArguments[0];

        public static EnumerablePropertySelector New<TClass, TProp>(Expression<Func<TClass, IEnumerable<TProp>>> lambda)
        {
            return new EnumerablePropertySelector { LambdaExpression = lambda };
        }
    }
}
