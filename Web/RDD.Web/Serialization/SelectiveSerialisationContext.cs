using System.Collections.Generic;
using System.Threading;
using RDD.Domain.Helpers.Expressions;

namespace RDD.Web.Serialization
{
    /// <summary>
    ///     Used to track the serialisation tree walkthrough
    /// </summary>
    public class SelectiveSerialisationContext
    {
        private static readonly AsyncLocal<SelectiveSerialisationContext> _context = new AsyncLocal<SelectiveSerialisationContext>();
        private readonly bool _serializeEverything;
        private readonly Stack<PropertyTreeNode> _stack;

        public SelectiveSerialisationContext(PropertyTreeNode root)
        {
            if (root == null)
            {
                _serializeEverything = true;
            }
            else
            {
                _stack = new Stack<PropertyTreeNode>();
                _stack.Push(root);
                _currentNode = root;
            }
        }

        public static SelectiveSerialisationContext Current
        {
            get => _context.Value;
            set => _context.Value = value;
        }

        private PropertyTreeNode _currentNode;
        private PropertyTreeNode _currentPropertyNode;

        private bool UntrackedMode => _currentNode.Children == null || _currentNode.Children.Count == 0;
        private int _untrackedLevel = 0;

        public void Push(string path)
        {
            if (UntrackedMode)
            {
                _untrackedLevel++;
                return;
            }
            if (_currentPropertyNode != null)
            {
                _stack.Push(_currentPropertyNode);
                _currentNode = _currentPropertyNode;
                _currentPropertyNode = null;
            }
        }

        public void Pop(string path)
        {
            if (_stack == null || _stack.Count == 0)
            {
                return;
            }
            if (UntrackedMode)
            {
                if (_untrackedLevel > 0)
                {
                    _untrackedLevel--;
                    return;
                }
            }
            _currentPropertyNode = _stack.Pop();
            _currentNode = _currentPropertyNode.ParentNode;
        }

        public bool IsCurrentNodeDefined(string propertyName)
        {
            if (_serializeEverything)
            {
                return true;
            }
            if (UntrackedMode)
            {
                return true;
            }
            if (_currentNode.Children.TryGetValue(propertyName, out var propNode))
            {
                _currentPropertyNode = propNode;
                return true;
            }
            return false;
        }
    }
}