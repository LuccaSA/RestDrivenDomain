using RDD.Domain.Helpers.Expressions.Utils;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace RDD.Domain.Helpers.Expressions
{
    public class PropertyExpressionSelector : IExpressionSelector
    {
        public LambdaExpression LambdaExpression { get; set; }

        public Expression Body => LambdaExpression?.Body;
        public MemberExpression MemberExpression => Body as MemberExpression;
        public PropertyInfo Property => MemberExpression?.Member as PropertyInfo;
        public string Name => Property?.Name;
        public virtual Type ResultType => Property.PropertyType;

        public virtual bool Equals(IExpressionSelector other)
            => other != null && ExpressionEqualityComparer.Eq(other.ToLambdaExpression(), LambdaExpression);

        LambdaExpression IExpressionSelector.ToLambdaExpression() => LambdaExpression;

        public override string ToString() => Name;
    }

    public static class PropertyExpressionSelector<TClass>
    {
        public static PropertyExpressionSelector New<TProp>(Expression<Func<TClass, TProp>> lambda)
            => new PropertyExpressionSelector { LambdaExpression = lambda };
    }
}