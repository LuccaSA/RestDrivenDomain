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
        private const char _propertiesSeparator = ',';
        private const char _fieldSeparator = '.';
        private const char _space = ' ';

        public static IEnumerable<string> Expand(string input)
        {
            return Expand(input, new Stack<StringBuilder>()).Select(i => i.ToString());
        }
        private static void EmptyBuffer(ref StringBuilder buffer, Stack<StringBuilder> prefixes, List<StringBuilder> analyseResult)
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

        private static void FeedBuffer(StringBuilder buffer, char c)
        {
            buffer.Append(c);
        }

        private static List<StringBuilder> Expand(string input, Stack<StringBuilder> prefixes)
        {
            var analyseResult = new List<StringBuilder>();
            var buffer = new StringBuilder();

            var isInsideFunction = false;
            var level = 0;

            for (var i = 0; i < input.Length; i++)
            {
                char character = input[i];
                switch (character)
                {
                    case _multiselectStart:
                        MultiStart(prefixes, ref buffer, ref level, character);
                        break;
                    case _multiselectEnd:
                        MultiEnd(prefixes, analyseResult, ref buffer, ref level, character);
                        break;
                    case _propertiesSeparator:
                        buffer = Separator(prefixes, analyseResult, buffer, isInsideFunction, level, character);
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
            EmptyBuffer(ref buffer, prefixes, analyseResult);
            return analyseResult;
        }

        private static StringBuilder Separator(Stack<StringBuilder> prefixes, List<StringBuilder> analyseResult, StringBuilder buffer, bool isInsideFunction, int level, char character)
        {
            if (level == 0 && !isInsideFunction)
            {
                EmptyBuffer(ref buffer, prefixes, analyseResult);
            }
            else
            {
                FeedBuffer(buffer, character);
            }
            return buffer;
        }

        private static void MultiEnd(Stack<StringBuilder> prefixes, List<StringBuilder> analyseResult, ref StringBuilder buffer, ref int level, char character)
        {
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