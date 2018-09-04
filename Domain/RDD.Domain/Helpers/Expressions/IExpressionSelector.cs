using System;
using System.Linq.Expressions;

namespace RDD.Domain.Helpers.Expressions
{
    public interface IExpressionSelector : IEquatable<IExpressionSelector>
    {
        string Name { get; }

        LambdaExpression ToLambdaExpression();

        Type ResultType { get; }
    }
}