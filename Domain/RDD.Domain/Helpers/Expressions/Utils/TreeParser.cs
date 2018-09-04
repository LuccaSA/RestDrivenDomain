using System;
using System.Collections.Generic;
using System.Linq;

namespace RDD.Domain.Helpers.Expressions.Utils
{
    internal class TreeParser
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