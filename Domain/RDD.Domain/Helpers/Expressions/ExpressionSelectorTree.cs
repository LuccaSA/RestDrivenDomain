using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace RDD.Domain.Helpers.Expressions
{
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
                case 1: return string.Join(".", new[] { start, Children[0].ToString() }.Where(e => !string.IsNullOrEmpty(e)));
                default: return start + "[" + string.Join(",", Children.Select(c => c.ToString())) + "]";
            }
        }

        public bool Equals(IExpressionSelectorTree other)
        {
            if (other == null && this == null)
            {
                return true;
            }

            if (other == null || this == null)
            {
                return false;
            }

            return new HashSet<IExpressionSelectorChain>(this, new ExpressionSelectorEqualityComparer()).SetEquals(other);
        }

        public bool Contains<TClass, TProp>(Expression<Func<TClass, TProp>> property)
            => Contains(new ExpressionSelectorParser().ParseChain(property));

        public bool Contains(IExpressionSelectorChain chain)
            => this.Any(c => c.Contains(chain));

        public IExpressionSelectorTree Intersection(IExpressionSelectorTree tree)
        {
            if (tree == null)
            {
                return null;
            }

            if (!(Node == null && tree.Node == null) && (Node == null || tree.Node == null || !Node.Equals(tree.Node)))
            {
                return null;
            }

            return new ExpressionSelectorTree { Node = Node, Children = Children.SelectMany(c => tree.Children.Select(t => c.Intersection(t))).Where(i => i != null).ToList() };
        }
    }
}