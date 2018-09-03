using RDD.Domain.Helpers.Expressions.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace RDD.Domain.Helpers.Expressions
{
    public interface IExpressionSelector : IEquatable<IExpressionSelector>
    {
        string Name { get; }
        LambdaExpression ToLambdaExpression();

        Type ResultType { get; }
    }

    public class SimplePropertySelector : IExpressionSelector
    {
        public LambdaExpression LambdaExpression { get; set; }

        public Expression Body => LambdaExpression?.Body;
        public MemberExpression MemberExpression => Body as MemberExpression;
        public PropertyInfo Property => MemberExpression?.Member as PropertyInfo;
        public string Name => Property?.Name;
        public virtual Type ResultType => Property.PropertyType;

        public static SimplePropertySelector New<TClass, TProp>(Expression<Func<TClass, TProp>> lambda)
        {
            return new SimplePropertySelector { LambdaExpression = lambda };
        }

        public bool Equals(IExpressionSelector other)
        {
            return (other == null && this == null)
                || (other != null && ExpressionEqualityComparer.Eq(other.ToLambdaExpression(), LambdaExpression));
        }

        LambdaExpression IExpressionSelector.ToLambdaExpression() => LambdaExpression;
    }

    class EnumerableMemberSelector : SimplePropertySelector
    {
        public override Type ResultType => Property.PropertyType.GenericTypeArguments[0];

        public static EnumerableMemberSelector New<TClass, TProp>(Expression<Func<TClass, IEnumerable<TProp>>> lambda)
        {
            return new EnumerableMemberSelector { LambdaExpression = lambda };
        }
    }

    class ItemSelector : IExpressionSelector
    {
        public LambdaExpression LambdaExpression { get; set; }

        public IndexExpression IndexExpression => LambdaExpression?.Body as IndexExpression;
        public PropertyInfo Property => IndexExpression?.Indexer;
        public string Name => (IndexExpression?.Arguments[0] as ConstantExpression)?.Value.ToString();

        public Type ResultType => Property.PropertyType;

        LambdaExpression IExpressionSelector.ToLambdaExpression() => LambdaExpression;

        public bool Equals(IExpressionSelector other)
        {
            return (other == null && this == null)
                || (other != null && ExpressionEqualityComparer.Eq(other.ToLambdaExpression(), LambdaExpression));
        }
    }

    public interface IExpressionSelectorChain : IExpressionSelector
    {
        IExpressionSelector Current { get; }
        IExpressionSelectorChain Next { get; }

        bool Contains(IExpressionSelectorChain chain);
    }

    public class ExpressionSelectorChain : IExpressionSelectorChain
    {
        public IExpressionSelector Current { get; set; }

        public IExpressionSelectorChain Next { get; set; }

        public string Name => Current.Name + "." + Next.Name;

        public Type ResultType => Next?.ResultType ?? Current.ResultType;

        public LambdaExpression ToLambdaExpression()
            => ExpressionChainer.Chain(Current?.ToLambdaExpression(), Next?.ToLambdaExpression());

        public static IExpressionSelectorChain New<TClass, TProp>(Expression<Func<TClass, TProp>> lambda)
            => new ExpressionSelectorParser<TClass>().ParseChain(lambda);

        public bool Contains(IExpressionSelectorChain chain)
        {
            return chain == null || (chain.Current.Equals(Current) && (chain.Next == null || (Next != null && Next.Contains(chain.Next))));
        }

        public bool Equals(IExpressionSelector other)
        {
            return (other == null && this == null)
                || (other != null && ExpressionEqualityComparer.Eq(other.ToLambdaExpression(), ToLambdaExpression()));
        }
    }

    public interface IExpressionSelectorTree : IEnumerable<IExpressionSelectorChain>
    {
        IExpressionSelector Node { get; }
        IEnumerable<IExpressionSelectorTree> Children { get; }
    }

    public class ExpressionSelectorTree : IExpressionSelectorTree
    {
        public IExpressionSelector Node { get; set; }

        IEnumerable<IExpressionSelectorTree> IExpressionSelectorTree.Children => Children;
        public List<IExpressionSelectorTree> Children { get; set; }

        public ExpressionSelectorTree()
        {
            Children = new List<IExpressionSelectorTree>();
        }

        public IEnumerator<IExpressionSelectorChain> GetEnumerator()
        {
            if (Node == null)
            {
                if (Children == null)
                {
                    return Enumerable.Empty<IExpressionSelectorChain>().GetEnumerator();
                }
                return Children.SelectMany(c => c).GetEnumerator();
            }
            if (Children == null || !Children.Any())
            {
                return new List<IExpressionSelectorChain> { new ExpressionSelectorChain { Current = Node } }.GetEnumerator();
            }
            return Children.SelectMany(s => s.Select(c => (IExpressionSelectorChain)new ExpressionSelectorChain { Current = Node, Next = c })).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
