using RDD.Domain.Helpers.Expressions.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace RDD.Domain.Helpers.Expressions
{
    public class ExpressionSelectorEqualityComparer : IEqualityComparer<IExpressionSelector>
    {
        public bool Equals(IExpressionSelector x, IExpressionSelector y)
        {
            return (x == null && y == null) || (x != null && x.Equals(y));
        }

        public int GetHashCode(IExpressionSelector obj) => (obj.Name.GetHashCode() * 23) + (obj.ResultType.GetHashCode() * 17);
    }

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

        public override string ToString() => Name;
    }

    class EnumerableMemberSelector : SimplePropertySelector
    {
        public override Type ResultType => Property.PropertyType.GenericTypeArguments[0];

        public static EnumerableMemberSelector New<TClass, TProp>(Expression<Func<TClass, IEnumerable<TProp>>> lambda)
        {
            return new EnumerableMemberSelector { LambdaExpression = lambda };
        }
    }

    public class ItemSelector : IExpressionSelector
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
        public override string ToString() => "[" + Name + "]";
    }

    public interface IExpressionSelectorChain : IExpressionSelector
    {
        IExpressionSelector Current { get; }
        IExpressionSelectorChain Next { get; }

        bool Contains<TClass, TProp>(Expression<Func<TClass, TProp>> property);
        bool Contains(IExpressionSelectorChain chain);
    }

    public class ExpressionSelectorChain : IExpressionSelectorChain
    {
        public IExpressionSelector Current { get; set; }

        public IExpressionSelectorChain Next { get; set; }

        public string Name => Current?.Name + (Next != null ? "." + Next.Name : "");

        public Type ResultType => Next?.ResultType ?? Current.ResultType;

        public LambdaExpression ToLambdaExpression()
            => ExpressionChainer.Chain(Current?.ToLambdaExpression(), Next?.ToLambdaExpression());

        public static IExpressionSelectorChain New<TClass, TProp>(Expression<Func<TClass, TProp>> lambda)
            => new ExpressionSelectorParser().ParseChain(lambda);

        public bool Contains<TClass, TProp>(Expression<Func<TClass, TProp>> property)
            => Contains(new ExpressionSelectorParser().ParseChain(property));

        public bool Contains(IExpressionSelectorChain chain)
        {
            return chain == null || (chain.Current.Equals(Current) && (chain.Next == null || (Next != null && Next.Contains(chain.Next))));
        }

        public bool Equals(IExpressionSelector other)
        {
            return (other == null && this == null)
                || (other != null && ExpressionEqualityComparer.Eq(other.ToLambdaExpression(), ToLambdaExpression()));
        }

        public override string ToString() => Name;
    }

    public interface IExpressionSelectorTree : IEnumerable<IExpressionSelectorChain>
    {
        IExpressionSelector Node { get; }
        IEnumerable<IExpressionSelectorTree> Children { get; }

        bool Contains<TClass, TProp>(Expression<Func<TClass, TProp>> property);
        bool Contains(IExpressionSelectorChain chain);
    }

    public class ExpressionSelectorTree : IExpressionSelectorTree
    {
        public IExpressionSelector Node { get; set; }

        IEnumerable<IExpressionSelectorTree> IExpressionSelectorTree.Children => Children;
        public List<IExpressionSelectorTree> Children { get; set; }

        public static IExpressionSelectorTree New<TClass, TProp>(Expression<Func<TClass, TProp>> lambda)
            => new ExpressionSelectorParser().ParseTree(lambda);

        public static IExpressionSelectorTree New<TClass, TProp1, TProp2>(Expression<Func<TClass, TProp1>> lambda1, Expression<Func<TClass, TProp2>> lambda2)
            => new ExpressionSelectorParser().ParseTree(lambda1, lambda2);

        public static IExpressionSelectorTree New<TClass, TProp1, TProp2, TProp3>(Expression<Func<TClass, TProp1>> lambda1, Expression<Func<TClass, TProp2>> lambda2, Expression<Func<TClass, TProp3>> lambda3)
            => new ExpressionSelectorParser().ParseTree(lambda1, lambda2, lambda3);

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

        public override string ToString()
        {
            var start = Node?.ToString();
            switch (Children.Count)
            {
                case 0: return start;
                case 1: return start + "." + Children[0].ToString();
                default: return start + "[" + string.Join(",", Children.Select(c => c.ToString())) + "]";
            }
        }

        public bool Contains<TClass, TProp>(Expression<Func<TClass, TProp>> property)
            => Contains(new ExpressionSelectorParser().ParseChain(property));

        public bool Contains(IExpressionSelectorChain chain)
            => this.Any(c => c.Contains(chain));
    }
}
