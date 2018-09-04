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
}
