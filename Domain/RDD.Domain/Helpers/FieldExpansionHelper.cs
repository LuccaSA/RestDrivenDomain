using System;
using System.Collections.Generic;
using System.Linq;

namespace RDD.Domain.Helpers
{
    public class FieldExpansionHelper
    {
        public FieldExpansionHelper()
            : this(new Stack<string>())
        {
        }

        private FieldExpansionHelper(Stack<string> prefixes)
        {
            _prefixes = prefixes;
            _buffer = string.Empty;
            _analyseResult = new List<string>();
        }

        private const char _multiselectStart = '[';
        private const char _multiselectEnd = ']';
        private const char _functionStart = '(';
        private const char _functionEnd = ')';
        private const char _propertiesSeparator = ',';
        private const char _fieldSeparator = '.';
        private const char _space = ' ';

        private readonly Stack<string> _prefixes;
        private readonly List<string> _analyseResult;
        private string _buffer;

        public List<string> Expand(string input)
        {
            var isInsideFunction = false;
            var level = 0;

            foreach (char character in input)
            {
                switch (character)
                {
                    case _multiselectStart:
                        if (level++ == 0)
                        {
                            _prefixes.Push(_buffer + _fieldSeparator);
                            _buffer = string.Empty;
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
                            _analyseResult.AddRange(new FieldExpansionHelper(_prefixes).Expand(_buffer));
                            _buffer = string.Empty;
                            _prefixes.Pop();
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
            return _analyseResult;
        }

        private void EmptyBuffer()
        {
            if (!string.IsNullOrWhiteSpace(_buffer))
            {
                _analyseResult.Add(string.Join(string.Empty, _prefixes.Reverse()) + _buffer);
                _buffer = string.Empty;
            }
        }

        private void FeedBuffer(char c)
        {
            _buffer += c;
        }
    }
}