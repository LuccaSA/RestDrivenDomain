using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RDD.Domain.Helpers.Expressions
{
    public class EnumerableMemberSelector : SimplePropertySelector
    {
        public override Type ResultType => Property.PropertyType.GenericTypeArguments[0];

        public static EnumerableMemberSelector New<TClass, TProp>(Expression<Func<TClass, IEnumerable<TProp>>> lambda)
        {
            return new EnumerableMemberSelector { LambdaExpression = lambda };
        }
    }
}
