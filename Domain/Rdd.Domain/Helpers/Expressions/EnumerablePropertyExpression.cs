using System;

namespace Rdd.Domain.Helpers.Expressions
{
    public class EnumerablePropertyExpression : PropertyExpression
    {
        public override Type ResultType => Property.PropertyType.GenericTypeArguments[0];
    }
}