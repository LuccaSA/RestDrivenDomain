using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RDD.Web.Serialization
{
    public static class QueryExpansionHelper
    {
        private const char _multiselectStart = '[';
        private const char _multiselectEnd = ']';
        private const char _functionStart = '(';
        private const char _functionEnd = ')';
        private const char _propertiesSeparator = ',';
        private const char _fieldSeparator = '.';
        private const char _space = ' ';

        public static IEnumerable<string> Expand(string input)
        {
            return Expand(input, new Stack<StringBuilder>()).Select(i => i.ToString());
        }

        private static List<StringBuilder> Expand(string input, Stack<StringBuilder> prefixes)
        {
            var analyseResult = new List<StringBuilder>();
            var buffer = new StringBuilder();

            void EmptyBuffer()
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
                    analyseResult.Add(prefixedBuffer);
                }
                else
                {
                    analyseResult.Add(buffer);
                }
                buffer = new StringBuilder();
            }

            void FeedBuffer(char c)
            {
                buffer.Append(c);
            }

            var isInsideFunction = false;
            var level = 0;

            for (var i = 0; i < input.Length; i++)
            {
                char character = input[i];
                switch (character)
                {
                    case _multiselectStart:
                        if (level++ == 0)
                        {
                            buffer.Append(_fieldSeparator);
                            prefixes.Push(buffer);
                            buffer = new StringBuilder();
                        }
                        else
                        {
                            FeedBuffer(character);
                        }
                        break;
                    case _multiselectEnd:
                        if (--level == 0)
                        {
                            if (level < 0)
                            {
                                throw new FormatException("Le champ fields est mal renseigné.");
                            }
                            var sub = Expand(buffer.ToString(), prefixes);
                            analyseResult.AddRange(sub);
                            buffer = new StringBuilder();
                            prefixes.Pop();
                        }
                        else
                        {
                            FeedBuffer(character);
                        }
                        break;
                    case _functionStart:
                        isInsideFunction = true;
                        FeedBuffer(character);
                        break;
                    case _functionEnd:
                        isInsideFunction = false;
                        FeedBuffer(character);
                        EmptyBuffer();
                        break;
                    case _propertiesSeparator:
                        if (level == 0 && !isInsideFunction)
                        {
                            EmptyBuffer();
                        }
                        else
                        {
                            FeedBuffer(character);
                        }

                        break;
                    case _space:
                        break;
                    default:
                        FeedBuffer(character);
                        break;
                }
            }

            if (level != 0)
            {
                throw new FormatException("Le champ fields est mal renseigné.");
            }
            EmptyBuffer();
            return analyseResult;
        }
    }
}