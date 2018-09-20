using System.Collections.Generic;
using System.Linq;

namespace RDD.Domain.Helpers.Expressions.Utils
{
    internal class Tree<T>
    {
        private readonly List<Tree<T>> _children;
        public IReadOnlyCollection<Tree<T>> Children => _children;

        public T Node { get; set; }

        public Tree(T node)
        {
            Node = node;
            _children = new List<Tree<T>>();
        }

        public override string ToString()
        {
            var start = Node?.ToString();
            switch (Children.Count)
            {
                case 0: return start;
                case 1: return string.Join(".", new[] { start, _children[0].ToString() }.Where(e => !string.IsNullOrEmpty(e)));
                default: return start + "[" + string.Join(",", _children.Select(c => c.ToString())) + "]";
            }
        }

        public void AddChild(Tree<T> child) => _children.Add(child);
        public void AddChildren(IEnumerable<Tree<T>> children) => _children.AddRange(children);
    }
}