using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Rdd.Domain.Helpers.Expressions
{
    public interface IExpressionTree : IEnumerable<IExpressionChain>, IEquatable<IExpressionTree>
    {
        IExpression Node { get; }
        IEnumerable<IExpressionTree> Children { get; }

        bool Contains<TClass, TProp>(Expression<Func<TClass, TProp>> property);
        bool Contains(IExpressionChain chain);

        IExpressionTree Intersection(IExpressionTree tree);
    }

    public interface IExpressionTree<TClass> : IExpressionTree
    {
        bool Contains<TProp>(Expression<Func<TClass, TProp>> property);
    }
}