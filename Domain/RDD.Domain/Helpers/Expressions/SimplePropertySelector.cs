using RDD.Domain.Helpers.Expressions.Utils;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace RDD.Domain.Helpers.Expressions
{
    public class SimplePropertySelector : IExpressionSelector
    {
        public LambdaExpression LambdaExpression { get; set; }

        public Expression Body => LambdaExpression?.Body;
        public MemberExpression MemberExpression => Body as MemberExpression;
        public PropertyInfo Property => MemberExpression?.Member as PropertyInfo;
        public string Name => Property?.Name;
        public virtual Type ResultType => Property.PropertyType;

        public static SimplePropertySelector New<TClass, TProp>(Expression<Func<TClass, TProp>> lambda)
        {
            return new SimplePropertySelector { LambdaExpression = lambda };
        }

        public bool Equals(IExpressionSelector other)
        {
            return (other == null && this == null)
                || (other != null && ExpressionEqualityComparer.Eq(other.ToLambdaExpression(), LambdaExpression));
        }

        LambdaExpression IExpressionSelector.ToLambdaExpression() => LambdaExpression;

        public override string ToString() => Name;
    }
}
