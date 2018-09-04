using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RDD.Domain.Helpers.Expressions
{
    public interface IExpressionSelectorTree : IEnumerable<IExpressionSelectorChain>, IEquatable<IExpressionSelectorTree>
    {
        IExpressionSelector Node { get; }
        IEnumerable<IExpressionSelectorTree> Children { get; }

        bool Contains<TClass, TProp>(Expression<Func<TClass, TProp>> property);
        bool Contains(IExpressionSelectorChain chain);

        IExpressionSelectorTree Intersection(IExpressionSelectorTree tree);
    }
}
