using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

namespace RDD.Web.Serialization
{
    /// <summary>
    /// Simple PropertyName graph structure
    /// Used to define a serialisation structure
    /// </summary>
    public class Node
    {
        public Node(Node parentNode, StringSegment name)
        {
            ParentNode = parentNode;
            Name = name;
        }

        public Node ParentNode { get; internal set; }
        /// <summary>
        /// Should corresponds to a property name, case insensitive
        /// </summary>
        public StringSegment Name { get; }
        internal Dictionary<StringSegment, Node> Children { get; set; }
        public string Path => ParentNode == null || StringSegment.IsNullOrEmpty(ParentNode.Path) ? Name.Value : ParentNode.Path + "." + Name;

        public Node GetOrCreateChildNode(StringSegment name)
        {
            if (Children == null)
            {
                Children = new Dictionary<StringSegment, Node>(StringSegmentComparer.OrdinalIgnoreCase);
            }
            else if (Children.TryGetValue(name, out Node found))
            {
                return found;
            }
            var newNode = new Node(this, name);
            Children.Add(name, newNode);
            return newNode;
        }

        public static Node NewRoot() => new Node(null, "") { Children = new Dictionary<StringSegment, Node>(StringSegmentComparer.OrdinalIgnoreCase) };

        public override string ToString()
        {
            return Path;
        }
    }
}