using System;
using System.Collections.Generic;
using System.Linq;

namespace RDD.Domain.Helpers.Expressions.Utils
{
    internal class TreeParser
    {
        private const char MULTISELECT_START = '[';
        private const char MULTISELECT_END = ']';
        private const char PROPERTIES_SEPARATOR = ',';
        private const char FIELD_SEPARATOR = '.';
        private const char SPACE = ' ';

        public Tree<string> Parse(string input)
        {
            var result = new Tree<string>(null);
            if (string.IsNullOrEmpty(input))
            {
                return result;
            }

            var offset = 0;
            var resetStackCounts = new Stack<int>();
            var currentTrees = new Stack<Tree<string>>();
            currentTrees.Push(result);

            for (var i = 0; i < input.Length; i++)
            {
                switch (input[i])
                {
                    case MULTISELECT_START:
                        PushNewSegment(input, ref offset, i, currentTrees, false);
                        resetStackCounts.Push(currentTrees.Count);
                        break;

                    case MULTISELECT_END:
                        if (resetStackCounts.Count == 0)
                        {
                            throw new FormatException("Input is not correctly formatted.");
                        }

                        PushNewSegment(input, ref offset, i, currentTrees, true);
                        SetCurrentTree(currentTrees, resetStackCounts.Peek());
                        resetStackCounts.Pop();
                        break;

                    case FIELD_SEPARATOR:
                        PushNewSegment(input, ref offset, i, currentTrees, false);
                        break;

                    case PROPERTIES_SEPARATOR:
                        PushNewSegment(input, ref offset, i, currentTrees, true);
                        SetCurrentTree(currentTrees, resetStackCounts.Count == 0 ? 1 : resetStackCounts.Peek());
                        break;

                    case SPACE:
                        if (offset != i)
                        {
                            throw new FormatException("Spaces are only allowed after separating characters.");
                        }

                        offset = i + 1;
                        break;
                }
            }

            if (resetStackCounts.Count != 0)
            {
                throw new FormatException("Input is not correctly formatted.");
            }

            PushNewSegment(input, ref offset, input.Length, currentTrees, true);

            return result;
        }

        void PushNewSegment(string input, ref int offset, int currentIndex, Stack<Tree<string>> currentTrees, bool allowEmpty)
        {
            if (currentIndex - offset > 0)
            {
                var currentSegment = input.Substring(offset, currentIndex - offset);
                var newCurrentTree = currentTrees.Peek().Children.FirstOrDefault(c => currentSegment.Equals(c.Node, StringComparison.OrdinalIgnoreCase));

                if (newCurrentTree == null)
                {
                    newCurrentTree = new Tree<string>(currentSegment);
                    currentTrees.Peek().AddChild(newCurrentTree);
                }

                currentTrees.Push(newCurrentTree);
            }
            else if (!allowEmpty)
            {
                throw new FormatException("Members cannot be empty");
            }

            offset = currentIndex + 1;
        }

        void SetCurrentTree(Stack<Tree<string>> currentTrees, int targetCount)
        {
            while (currentTrees.Count != targetCount)
            {
                currentTrees.Pop();
            }
        }
    }
}