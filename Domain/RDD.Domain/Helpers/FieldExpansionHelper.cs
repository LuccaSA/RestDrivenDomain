using System;
using System.Collections.Generic;
using System.Linq;

namespace RDD.Domain.Helpers
{
	public class FieldExpansionHelper
	{
	    private const char MULTISELECT_START = '[';
	    private const char MULTISELECT_END = ']';
	    private const char FUNCTION_START = '(';
	    private const char FUNCTION_END = ')';
	    private const char PROPERTIES_SEPARATOR = ',';
	    private const char FIELD_SEPARATOR = '.';
	    private const char SPACE = ' ';

	    private Stack<string> prefixes { get; }
	    private List<string> analyseResult { get; }
	    private string buffer { get; set; }

		public FieldExpansionHelper()
			: this(new Stack<string>()) { }

	    private FieldExpansionHelper(Stack<string> prefixes)
		{
			this.prefixes = prefixes;
			buffer = string.Empty;
			analyseResult = new List<string>();
		}

	    private void EmptyBuffer()
		{
			if (!string.IsNullOrWhiteSpace(buffer))
			{
				analyseResult.Add(string.Join(string.Empty, prefixes.Reverse()) + buffer);
				buffer = string.Empty;
			}
		}

	    private void FeedBuffer(char c) { buffer += c; }

		public List<string> Expand(string input)
		{
			var isInsideFunction = false;
			var level = 0;

			foreach (var character in input)
			{
				switch (character)
				{
					case MULTISELECT_START:
						if (level++ == 0)
						{
							prefixes.Push(buffer + FIELD_SEPARATOR);
							buffer = string.Empty;
						}
						else
						{
							FeedBuffer(character);
						}
						break;
					case MULTISELECT_END:
						if (--level == 0)
						{
							if (level < 0) { throw new FormatException("Le champ fields est mal renseigné."); }
							analyseResult.AddRange(new FieldExpansionHelper(prefixes).Expand(buffer));
							buffer = string.Empty;
							prefixes.Pop();
						}
						else
						{
							FeedBuffer(character);
						}
						break;
					case FUNCTION_START:
						isInsideFunction = true;
						FeedBuffer(character);
						break;
					case FUNCTION_END:
						isInsideFunction = false;
						FeedBuffer(character);
						EmptyBuffer();
						break;
					case PROPERTIES_SEPARATOR:
						if (level == 0 && !isInsideFunction)
						{
							EmptyBuffer();
						}
						else
						{
							FeedBuffer(character);
						}
						break;
					case SPACE:
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

