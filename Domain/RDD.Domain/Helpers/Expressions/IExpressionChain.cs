using System;
using System.Linq.Expressions;

namespace Rdd.Domain.Helpers.Expressions
{
    public interface IExpressionChain : IExpression
    {
        IExpression Current { get; }
        IExpressionChain Next { get; }

        bool Contains<TClass, TProp>(Expression<Func<TClass, TProp>> property);
        bool Contains(IExpressionChain chain);
    }

    public interface IExpressionChain<TClass> : IExpressionChain
    {
        bool Contains<TProp>(Expression<Func<TClass, TProp>> property);
    }

    public static class IExpressionChainExtensions
    {
        public static bool HasNext(this IExpressionChain chain) => chain?.Next != null;
    }
}