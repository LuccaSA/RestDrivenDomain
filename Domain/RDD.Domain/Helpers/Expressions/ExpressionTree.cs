using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Rdd.Domain.Helpers.Expressions
{
    public class ExpressionTree : IExpressionTree
    {
        public IExpression Node { get; set; }

        IReadOnlyCollection<IExpressionTree> IExpressionTree.Children => Children;
        public List<IExpressionTree> Children { get; set; }

        public ExpressionTree()
        {
            Children = new List<IExpressionTree>();
        }

        public IEnumerator<IExpressionChain> GetEnumerator()
        {
            if (Node == null)
            {
                if (Children == null)
                {
                    return Enumerable.Empty<IExpressionChain>().GetEnumerator();
                }
                return Children.SelectMany(c => c).GetEnumerator();
            }
            if (Children == null || !Children.Any())
            {
                return new List<IExpressionChain> { new ExpressionChain { Current = Node } }.GetEnumerator();
            }
            return Children.SelectMany(s => s.Select(c => (IExpressionChain)new ExpressionChain { Current = Node, Next = c })).GetEnumerator();
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

        public virtual bool Equals(IExpressionTree other)
            => other != null && new HashSet<IExpressionChain>(this, new RddExpressionEqualityComparer()).SetEquals(other);

        public bool Contains<TClass, TProp>(Expression<Func<TClass, TProp>> property)
            => Contains(new ExpressionParser().ParseChain(property));

        public bool Contains(IExpressionChain chain)
            => this.Any(c => c.Contains(chain));

        public IExpressionTree Intersection(IExpressionTree tree)
        {
            if (tree == null)
            {
                return null;
            }

            if (!(Node == null && tree.Node == null) && (Node == null || tree.Node == null || !Node.Equals(tree.Node)))
            {
                return null;
            }

            return new ExpressionTree { Node = Node, Children = Children.SelectMany(c => tree.Children.Select(t => c.Intersection(t))).Where(i => i != null).ToList() };
        }
    }

    public class ExpressionTree<TClass> : ExpressionTree, IExpressionTree<TClass>
    {
        public static IExpressionTree<TClass> New<TProp>(Expression<Func<TClass, TProp>> lambda)
            => new ExpressionParser().ParseTree(lambda);

        public static IExpressionTree<TClass> New<TProp1, TProp2>(Expression<Func<TClass, TProp1>> lambda1, Expression<Func<TClass, TProp2>> lambda2)
            => new ExpressionParser().ParseTree(lambda1, lambda2);

        public static IExpressionTree<TClass> New<TProp1, TProp2, TProp3>(Expression<Func<TClass, TProp1>> lambda1, Expression<Func<TClass, TProp2>> lambda2, Expression<Func<TClass, TProp3>> lambda3)
            => new ExpressionParser().ParseTree(lambda1, lambda2, lambda3);

        public static IExpressionTree<TClass> New<TProp1, TProp2, TProp3, TProp4>(Expression<Func<TClass, TProp1>> lambda1, Expression<Func<TClass, TProp2>> lambda2, Expression<Func<TClass, TProp3>> lambda3, Expression<Func<TClass, TProp4>> lambda4)
            => new ExpressionParser().ParseTree(lambda1, lambda2, lambda3, lambda4);

        public bool Contains<TProp>(Expression<Func<TClass, TProp>> property)
            => base.Contains(property);    
    }
}