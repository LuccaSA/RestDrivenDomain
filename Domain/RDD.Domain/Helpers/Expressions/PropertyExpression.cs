using RDD.Domain.Helpers.Expressions.Equality;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace RDD.Domain.Helpers.Expressions
{
    public class PropertyExpression : IExpression
    {
        public LambdaExpression LambdaExpression { get; set; }

        public Expression Body => LambdaExpression?.Body;
        public MemberExpression MemberExpression => Body as MemberExpression;
        public PropertyInfo Property => MemberExpression?.Member as PropertyInfo;
        public string Name => Property?.Name;
        public virtual Type ResultType => Property.PropertyType;

        public virtual bool Equals(IExpression other)
            => other != null && new ExpressionEqualityComparer().Equals(other.ToLambdaExpression(), LambdaExpression);

        LambdaExpression IExpression.ToLambdaExpression() => LambdaExpression;

        public override string ToString() => Name;
    }

    public static class PropertyExpression<TClass>
    {
        public static PropertyExpression New<TProp>(Expression<Func<TClass, TProp>> lambda)
            => new PropertyExpression { LambdaExpression = lambda };
    }
}