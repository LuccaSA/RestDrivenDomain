using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Primitives;

namespace RDD.Web.Serialization
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

        private PropertyTreeNode(StringSegment name, Dictionary<StringSegment, PropertyTreeNode> children)
        {
            Name = name;
            Children = children;
        }

        public PropertyTreeNode ParentNode { get; internal set; }

        /// <summary>
        /// Should corresponds to a property name, case insensitive
        /// </summary>
        public StringSegment Name { get; }

        internal Dictionary<StringSegment, PropertyTreeNode> Children { get; set; }

        public string Path
        {
            get { return ParentNode == null || StringSegment.IsNullOrEmpty(ParentNode.Path) ? Name.Value : ParentNode.Path + "." + Name; }
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



        public PropertyTreeNode Intersect(PropertyTreeNode other) => Intersect(this, other);

        public PropertyTreeNode Intersect(IEnumerable<string> fullPathList)
        {
            return Intersect(this, fullPathList.ParseNode());
        }

        private static PropertyTreeNode Intersect(PropertyTreeNode left, PropertyTreeNode right)
        {
            var newTree = NewRoot();

            var queueLeft = new Queue<PropertyTreeNode>();
            var queueRight = new Queue<PropertyTreeNode>();
            var queue = new Queue<PropertyTreeNode>();

            queueLeft.Enqueue(left);
            queueRight.Enqueue(right);
            queue.Enqueue(newTree);

            while (queueLeft.Any())
            {
                var nextLeft = queueLeft.Dequeue();
                var nextRight = queueLeft.Dequeue();
                var current = queue.Dequeue();

                if (nextLeft.Name != nextRight.Name)
                {
                    continue;
                }

                foreach (var child in nextLeft.Children)
                {
                    if (!nextRight.Children.ContainsKey(child.Key))
                    {
                        continue;
                    }

                    queueLeft.Enqueue(child.Value);
                    queueRight.Enqueue(nextRight.Children[child.Key]);
                    queue.Enqueue(current.GetOrCreateChildNode(child.Key));
                }
            }

            return newTree;
        }

    }



      
}