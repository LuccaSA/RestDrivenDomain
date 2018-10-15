using Newtonsoft.Json.Serialization;
using Rdd.Domain.Helpers.Expressions.Equality;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Rdd.Domain.Helpers.Expressions
{
    public class PropertyExpression : IExpression
    {
        public IValueProvider ValueProvider { get; }
        public LambdaExpression LambdaExpression { get; }

        public PropertyInfo Property { get; }
        public string Name => Property?.Name;
        public virtual Type ResultType => Property.PropertyType;

        public PropertyExpression(LambdaExpression lambdaExpression)
        {
            LambdaExpression = lambdaExpression;
            Property = (LambdaExpression?.Body as MemberExpression).Member as PropertyInfo;
            ValueProvider = new ExpressionValueProvider(Property);
        }

        public virtual bool Equals(IExpression other)
            => other != null && new ExpressionEqualityComparer().Equals(other.ToLambdaExpression(), LambdaExpression);

        LambdaExpression IExpression.ToLambdaExpression() => LambdaExpression;

        public override string ToString() => Name;
    }

    public static class PropertyExpression<TClass>
    {
        public static PropertyExpression New<TProp>(Expression<Func<TClass, TProp>> lambda)
            => new PropertyExpression(lambda);
    }
}