using System;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

namespace RDD.Web.Serialization
{
    public static class NodeHelper
    {
        public static Node ParseNode(this IEnumerable<string> propertylist)
        {
            if (propertylist == null)
            {
                throw new ArgumentNullException(nameof(propertylist));
            }

            Node root = Node.NewRoot();

            foreach (var path in propertylist)
            {
                root.AddParsedSegments(path);
            }

            return root;
        }

        private static void AddParsedSegments(this Node node, StringSegment path)
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
                    var newnode = currentNode.GetOrCreateChildNode(sub);
                    currentNode = newnode;
                    path = path.Subsegment(i + 1, path.Length - (i + 1));
                    continue;
                }
                break;
            }
        }
    }
}