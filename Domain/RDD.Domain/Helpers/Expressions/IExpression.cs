using System;
using System.Linq.Expressions;

namespace RDD.Domain.Helpers.Expressions
{
    public interface IExpression : IEquatable<IExpression>
    {
        string Name { get; }

        LambdaExpression ToLambdaExpression();

        Type ResultType { get; }
    }
}