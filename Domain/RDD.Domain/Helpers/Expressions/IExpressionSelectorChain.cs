using System;
using System.Linq.Expressions;

namespace RDD.Domain.Helpers.Expressions
{
    public interface IExpressionSelectorChain : IExpressionSelector
    {
        IExpressionSelector Current { get; }
        IExpressionSelectorChain Next { get; }

        bool Contains<TClass, TProp>(Expression<Func<TClass, TProp>> property);
        bool Contains(IExpressionSelectorChain chain);
    }

    public interface IExpressionSelectorChain<TClass> : IExpressionSelectorChain
    {
        bool Contains<TProp>(Expression<Func<TClass, TProp>> property);
    }

    public static class IExpressionSelectorChainExtensions
    {
        public static bool HasNext(this IExpressionSelectorChain chain) => chain?.Next != null;
    }
}