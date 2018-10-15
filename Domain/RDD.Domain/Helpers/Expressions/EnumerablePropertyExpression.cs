using System;
using System.Linq.Expressions;

namespace Rdd.Domain.Helpers.Expressions
{
    public class EnumerablePropertyExpression : PropertyExpression
    {
        public EnumerablePropertyExpression(LambdaExpression lambdaExpression) : base(lambdaExpression)
        {
        }

        public override Type ResultType => Property.PropertyType.GenericTypeArguments[0];
    }
}