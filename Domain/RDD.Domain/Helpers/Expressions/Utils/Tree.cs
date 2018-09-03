using System;
using System.Collections.Generic;
using System.Linq;

namespace RDD.Domain.Helpers.Expressions.Utils
{
    public abstract class Tree
    {
        public abstract object GetNode();
        public abstract IReadOnlyCollection<Tree> GetChildren();
    }

    public class Tree<T> : Tree
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

    public class TreeParser
    {
        const char MULTISELECT_START = '[';
        const char MULTISELECT_END = ']';
        const char PROPERTIES_SEPARATOR = ',';
        const char FIELD_SEPARATOR = '.';
        const char SPACE = ' ';

        public const string Root = "__root__";

        public Tree<string> Parse(string input)
        {
            var result = new Tree<string>(Root);

            if (!string.IsNullOrEmpty(input))
            {
                var treeStack = new Stack<Tuple<char, List<Tree<string>>>>();
                AddLevel(treeStack, SPACE);

                foreach (var character in input)
                {
                    switch (character)
                    {
                        case MULTISELECT_START:
                            AddLevel(treeStack, MULTISELECT_START);
                            break;

                        case MULTISELECT_END:
                            CleanDottedElements(treeStack);

                            var pop = treeStack.Pop();
                            if (pop.Item1 != MULTISELECT_START)
                            {
                                throw new FormatException("bad format");
                            }

                            treeStack.Peek().Item2.Last().AddChildren(pop.Item2);
                            break;

                        case FIELD_SEPARATOR:
                            AddLevel(treeStack, FIELD_SEPARATOR);
                            break;

                        case PROPERTIES_SEPARATOR:
                            CleanDottedElements(treeStack);
                            treeStack.Peek().Item2.Add(new Tree<string>(""));
                            break;

                        case SPACE: break;

                        default:
                            AddCharacter(treeStack, character);
                            break;
                    }
                }

                CleanDottedElements(treeStack);

                if (treeStack.Count != 1)
                    throw new FormatException("bad format");

                result.AddChildren(treeStack.Pop().Item2);
            }
            return result;
        }

        void AddCharacter(Stack<Tuple<char, List<Tree<string>>>> stack, char character)
        {
            stack.Peek().Item2.Last().Node += character;
        }

        void CleanDottedElements(Stack<Tuple<char, List<Tree<string>>>> stack)
        {
            while (stack.Peek().Item1 == FIELD_SEPARATOR)
            {
                var pop = stack.Pop().Item2;
                stack.Peek().Item2.Last().AddChildren(pop);
            }
        }

        void AddLevel(Stack<Tuple<char, List<Tree<string>>>> stack, char initiator)
        {
            stack.Push(new Tuple<char, List<Tree<string>>>(initiator, new List<Tree<string>> { new Tree<string>("") }));
        }
    }
}