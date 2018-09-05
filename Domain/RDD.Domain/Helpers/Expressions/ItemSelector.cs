using RDD.Domain.Helpers.Expressions.Utils;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace RDD.Domain.Helpers.Expressions
{
    public class ItemSelector : IExpressionSelector
    {
        public LambdaExpression LambdaExpression { get; set; }

        public IndexExpression IndexExpression => LambdaExpression?.Body as IndexExpression;
        public PropertyInfo Property => IndexExpression?.Indexer;
        public string Name => (IndexExpression?.Arguments[0] as ConstantExpression)?.Value.ToString();

        public Type ResultType => Property.PropertyType;

        LambdaExpression IExpressionSelector.ToLambdaExpression() => LambdaExpression;

        public virtual bool Equals(IExpressionSelector other)
            => other != null && ExpressionEqualityComparer.Eq(other.ToLambdaExpression(), LambdaExpression);

        public override string ToString() => "[" + Name + "]";
    }
}