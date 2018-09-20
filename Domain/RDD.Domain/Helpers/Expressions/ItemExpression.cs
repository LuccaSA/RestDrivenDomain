using RDD.Domain.Helpers.Expressions.Equality;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace RDD.Domain.Helpers.Expressions
{
    public class ItemExpression : IExpression
    {
        public LambdaExpression LambdaExpression { get; set; }

        public IndexExpression IndexExpression => LambdaExpression?.Body as IndexExpression;
        public PropertyInfo Property => IndexExpression?.Indexer;
        public string Name => (IndexExpression?.Arguments[0] as ConstantExpression)?.Value.ToString();

        public Type ResultType => Property.PropertyType;

        LambdaExpression IExpression.ToLambdaExpression() => LambdaExpression;

        public virtual bool Equals(IExpression other)
            => other != null && new ExpressionEqualityComparer().Equals(other.ToLambdaExpression(), LambdaExpression);

        public override string ToString() => "[" + Name + "]";
    }
}