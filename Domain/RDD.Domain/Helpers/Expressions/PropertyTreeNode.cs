using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Primitives;

namespace RDD.Domain.Helpers.Expressions
{
    /// <summary>
    /// Simple PropertyName graph structure
    /// Used to define a serialisation structure
    /// </summary>
    public class PropertyTreeNode
    {
        public PropertyTreeNode(PropertyTreeNode parentNode, StringSegment name)
        {
            ParentNode = parentNode;
            Name = name;
        }

        public PropertyTreeNode ParentNode { get; private set; }

        /// <summary>
        /// Should corresponds to a property name, case insensitive
        /// </summary>
        public StringSegment Name { get; }

        public Dictionary<StringSegment, PropertyTreeNode> Children { get; set; }

        public string Path
        {
            get
            {
                var stack = new Stack<PropertyTreeNode>();
                PropertyTreeNode current = this;
                while (current != null)
                {
                    stack.Push(current);
                    current = current.ParentNode;
                }
                var sb = new StringBuilder();
                while (stack.Count != 0)
                {
                    PropertyTreeNode node = stack.Pop();
                    if (StringSegment.IsNullOrEmpty(node.Name))
                    {
                        continue;
                    }
                    sb.Append(node.Name);
                    if (stack.Count != 0)
                    {
                        sb.Append('.');
                    }
                }
                return sb.ToString();
            }
        }

        public PropertyTreeNode GetOrCreateChildNode(StringSegment name)
        {
            if (Children == null)
            {
                Children = new Dictionary<StringSegment, PropertyTreeNode>(StringSegmentComparer.OrdinalIgnoreCase);
            }
            else if (Children.TryGetValue(name, out PropertyTreeNode found))
            {
                return found;
            }

            var newNode = new PropertyTreeNode(this, name);
            Children.Add(name, newNode);
            return newNode;
        }

        public static PropertyTreeNode NewRoot() => new PropertyTreeNode(null, "") { Children = new Dictionary<StringSegment, PropertyTreeNode>(StringSegmentComparer.OrdinalIgnoreCase) };

        public override string ToString() => Path;

        public PropertyTreeNode Reparent(PropertyTreeNode newParent)
        {
            newParent.Children = Children;
            foreach (var n in newParent.Children.Values)
            {
                n.ParentNode = newParent;
            }
            return newParent;
        }

        public PropertyTreeNode Intersection(PropertyTreeNode other) => this.Intersect(other);

        public PropertyTreeNode Intersection(IEnumerable<string> fullPathList) => this.Intersect(fullPathList.ParseNode());

        public static PropertyTreeNode ParseFields(StringValues values) => values.SelectMany(QueryExpansionHelper.Expand).ParseNode();
    }

    public static class PropertyTreeNodeExtensions
    {
        public static IEnumerable<string> AsExpandedPaths(this PropertyTreeNode root)
        {
            if (root?.Children == null || root.Children.Count == 0)
            {
                yield break;
            }
            var stack = new Stack<PropertyTreeNode>();
            stack.Push(root);
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (current.Children == null || current.Children.Count == 0)
                {
                    yield return current.Path;
                    continue;
                }
                foreach (var child in current.Children)
                {
                    stack.Push(child.Value);
                }
            }
        }

        private class TypedNode
        {
            public TypedNode(PropertyTreeNode node, Type type)
            {
                Node = node ?? throw new ArgumentNullException(nameof(node));
                Type = type ?? throw new ArgumentNullException(nameof(type));
            }

            public PropertyTreeNode Node { get; }
            public Type Type { get; }
        }

        public static IEnumerable<string> AsExpandedPaths<T>(this PropertyTreeNode root)
            where T : class
        {
            if (root?.Children == null || root.Children.Count == 0)
            {
                yield break;
            }
            var stack = new Stack<TypedNode>();

            stack.Push(new TypedNode(root, typeof(T)));

            while (stack.Count != 0)
            {
                var current = stack.Pop();

                if (current.Node.Children == null || current.Node.Children.Count == 0)
                {
                    yield return current.Node.Path;
                    continue;
                }

                foreach (var child in current.Node.Children)
                {
                    var property = current.Type.GetProperty(child.Key.Value, _propertySearch);
                    if (property == null)
                    {
                        continue;
                    }
                    stack.Push(new TypedNode(child.Value, property.PropertyType));
                }
            }
        }

        private static BindingFlags _propertySearch = BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.GetProperty | BindingFlags.Instance;

        private class IntersectionNode
        {
            public IntersectionNode(PropertyTreeNode left, PropertyTreeNode right, PropertyTreeNode target)
            {
                Left = left ?? throw new ArgumentNullException(nameof(left));
                Right = right ?? throw new ArgumentNullException(nameof(right));
                Target = target ?? throw new ArgumentNullException(nameof(target));
            }

            public PropertyTreeNode Left { get; }
            public PropertyTreeNode Right { get; }
            public PropertyTreeNode Target { get; }
        }

        public static PropertyTreeNode Intersect(this PropertyTreeNode left, PropertyTreeNode right)
        {
            if (left == null)
            {
                throw new ArgumentNullException(nameof(left));
            }
            if (right == null)
            {
                throw new ArgumentNullException(nameof(right));
            }

            var newTree = PropertyTreeNode.NewRoot();
            var queue = new Queue<IntersectionNode>();
            queue.Enqueue(new IntersectionNode(left, right, newTree));

            while (queue.Count != 0)
            {
                var current = queue.Dequeue();

                if (current.Left.Name != current.Right.Name)
                {
                    continue;
                }

                foreach (var child in current.Left.Children)
                {
                    if (!current.Right.Children.ContainsKey(child.Key))
                    {
                        continue;
                    }

                    queue.Enqueue(new IntersectionNode(
                            child.Value,
                            current.Right.Children[child.Key],
                            current.Target.GetOrCreateChildNode(child.Key)));
                }
            }

            return newTree;
        }
    }

    public static class NodeHelper
    {
        public static PropertyTreeNode ParseNode(this IEnumerable<string> propertyList)
        {
            if (propertyList == null)
            {
                throw new ArgumentNullException(nameof(propertyList));
            }

            var root = PropertyTreeNode.NewRoot();

            foreach (var path in propertyList)
            {
                root.AddParsedSegments(path);
            }

            return root;
        }

        private static void AddParsedSegments(this PropertyTreeNode node, StringSegment path)
        {
            var currentNode = node;
            while (true)
            {
                int i = path.IndexOf('.');
                if (i == -1)
                {
                    currentNode.GetOrCreateChildNode(path);
                }
                else
                {
                    var sub = path.Subsegment(0, i);
                    var newNode = currentNode.GetOrCreateChildNode(sub);
                    currentNode = newNode;
                    path = path.Subsegment(i + 1, path.Length - (i + 1));
                    continue;
                }
                break;
            }
        }
    }

    public static class QueryExpansionHelper
    {
        private const char _multiSelectStart = '[';
        private const char _multiSelectEnd = ']';
        private const char _propertiesSeparator = ',';
        private const char _fieldSeparator = '.';
        private const char _space = ' ';

        public static IEnumerable<string> Expand(string input)
        {
            return Expand(input, new Stack<StringBuilder>()).Select(i => i.ToString());
        }

        private static void EmptyBuffer(ref StringBuilder buffer, Stack<StringBuilder> prefixes, List<StringBuilder> analysisResult)
        {
            if (buffer.Length == 0)
            {
                return;
            }
            if (prefixes.Any())
            {
                var prefixedBuffer = new StringBuilder();
                foreach (var v in prefixes.Reverse())
                {
                    prefixedBuffer.Append(v);
                }
                prefixedBuffer.Append(buffer);
                analysisResult.Add(prefixedBuffer);
            }
            else
            {
                analysisResult.Add(buffer);
            }
            buffer = new StringBuilder();
        }

        private static void FeedBuffer(StringBuilder buffer, char c)
        {
            buffer.Append(c);
        }

        private static List<StringBuilder> Expand(string input, Stack<StringBuilder> prefixes)
        {
            var analysisResult = new List<StringBuilder>();
            var buffer = new StringBuilder();

            var level = 0;

            for (var i = 0; i < input.Length; i++)
            {
                char character = input[i];
                switch (character)
                {
                    case _multiSelectStart:
                        MultiStart(prefixes, ref buffer, ref level, character);
                        break;
                    case _multiSelectEnd:
                        MultiEnd(prefixes, analysisResult, ref buffer, ref level, character);
                        break;
                    case _propertiesSeparator:
                        buffer = Separator(prefixes, analysisResult, buffer, level, character);
                        break;
                    case _space:
                        break;
                    default:
                        FeedBuffer(buffer, character);
                        break;
                }
            }

            if (level != 0)
            {
                throw new FormatException("Le champ fields est mal renseigné.");
            }
            EmptyBuffer(ref buffer, prefixes, analysisResult);
            return analysisResult;
        }

        private static StringBuilder Separator(Stack<StringBuilder> prefixes, List<StringBuilder> analysisResult, StringBuilder buffer, int level, char character)
        {
            if (level == 0)
            {
                EmptyBuffer(ref buffer, prefixes, analysisResult);
            }
            else
            {
                FeedBuffer(buffer, character);
            }
            return buffer;
        }

        private static void MultiEnd(Stack<StringBuilder> prefixes, List<StringBuilder> analysisResult, ref StringBuilder buffer, ref int level, char character)
        {
            if (--level == 0)
            {
                if (level < 0)
                {
                    throw new FormatException("Le champ fields est mal renseigné.");
                }
                var sub = Expand(buffer.ToString(), prefixes);
                analysisResult.AddRange(sub);
                buffer = new StringBuilder();
                prefixes.Pop();
            }
            else
            {
                FeedBuffer(buffer, character);
            }
        }

        private static void MultiStart(Stack<StringBuilder> prefixes, ref StringBuilder buffer, ref int level, char character)
        {
            if (level++ == 0)
            {
                buffer.Append(_fieldSeparator);
                prefixes.Push(buffer);
                buffer = new StringBuilder();
            }
            else
            {
                FeedBuffer(buffer, character);
            }
        }
    }


}