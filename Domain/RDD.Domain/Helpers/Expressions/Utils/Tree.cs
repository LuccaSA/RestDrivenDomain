using System.Collections.Generic;
using System.Linq;

namespace RDD.Domain.Helpers.Expressions.Utils
{
    internal abstract class Tree
    {
        public abstract object GetNode();
        public abstract IReadOnlyCollection<Tree> GetChildren();
    }

    internal class Tree<T> : Tree
    {
        private List<Tree<T>> _children { get; set; }

        public IReadOnlyCollection<Tree<T>> Children { get { return _children; } }

        public T Node { get; set; }

        internal protected Tree<T> Parent { get; protected set; }

        public Tree() : this(default(T)) { }
        public Tree(T node) : this(node, new List<Tree<T>>()) { }
        public Tree(T node, IEnumerable<Tree<T>> children)
        {
            Node = node;
            _children = new List<Tree<T>>();
            AddChildren(children);
        }

        public bool IsLeaf()
        {
            return Children == null || Children.Count == 0;
        }

        public int CountLeaves()
        {
            return IsLeaf() ? 1 : Children.Sum(child => child.CountLeaves());
        }

        public Tree<T> GetParent() { return Parent; }
        public override object GetNode() { return Node; }
        public override IReadOnlyCollection<Tree> GetChildren() { return Children; }
        public override string ToString()
        {
            return Node == null ? base.ToString() : Node.ToString();
        }

        public void AddChild(Tree<T> child)
        {
            child.Parent = this;
            _children.Add(child);
        }

        public int GetChildIndex(Tree<T> child)
        {
            return _children.IndexOf(child);
        }

        public void InsertChild(int index, Tree<T> child)
        {
            child.Parent = this;
            _children.Insert(index, child);
        }

        public void AddChildren(IEnumerable<Tree<T>> children)
        {
            foreach (var child in children)
            {
                AddChild(child);
            }
        }

        public bool Detach()
        {
            return Parent == null || Parent.Children == null || Parent._children.Remove(this);
        }
    }
}