using RDD.Domain.Helpers.Expressions.Utils;
using System;
using System.Linq.Expressions;

namespace RDD.Domain.Helpers.Expressions
{
    public class MethodCallSelector : IExpressionSelector
    {
        public LambdaExpression LambdaExpression { get; set; }
        public MethodCallExpression MethodCallExpression => LambdaExpression?.Body as MethodCallExpression;
        public string Name => MethodCallExpression.Method.Name;

        public Type ResultType => MethodCallExpression.Method.ReturnType;

        public bool Equals(IExpressionSelector other)
        {
            return (other == null && this == null)
                || (other != null && ExpressionEqualityComparer.Eq(other.ToLambdaExpression(), LambdaExpression));
        }

        public LambdaExpression ToLambdaExpression() => LambdaExpression;

        public override string ToString() => Name + "()";
    }
}
