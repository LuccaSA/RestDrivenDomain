using System.Collections.Generic;
using System.Threading;

namespace RDD.Web.Serialization
{
    /// <summary>
    ///     Used to track the serialisation tree walkthrough
    /// </summary>
    public class SelectiveSerialisationContext
    {
        private static readonly AsyncLocal<SelectiveSerialisationContext> _context = new AsyncLocal<SelectiveSerialisationContext>();
        private readonly bool _serializeEverything;
        private readonly Stack<Node> _stack;

        public SelectiveSerialisationContext(Node root)
        {
            if (root == null)
            {
                _serializeEverything = true;
            }
            else
            {
                _stack = new Stack<Node>();
                _stack.Push(root);
                _currentNode = root;
            }
        }

        public static SelectiveSerialisationContext Current
        {
            get => _context.Value;
            set => _context.Value = value;
        }

        private Node _currentNode;
        private Node _currentPropertyNode;
        private int _untrackedDepth;
        private bool _untrackedMode;

        public void Push()
        {
            if (_untrackedMode)
            {
                _untrackedDepth++;
                return;
            }
            if (_currentPropertyNode != null)
            {
                _stack.Push(_currentPropertyNode);
                _currentNode = _currentPropertyNode;
                _currentPropertyNode = null;
            }
        }

        public void Pop()
        {
            if (_untrackedMode)
            {
                _untrackedDepth--;
                if (_untrackedDepth == 0)
                {
                    _untrackedMode = false;
                }
                return;
            }
            if (_stack == null || _stack.Count == 0)
            {
                return;
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

            if (_untrackedMode)
            {
                return true;
            }
            if (_currentNode.Children == null)
            {
                _untrackedMode = true;
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