using System;
using System.Collections.Generic;
using System.Linq;

namespace RDD.Domain.Helpers
{
	public class FieldExpansionHelper
	{
		const char MULTISELECT_START = '[';
		const char MULTISELECT_END = ']';
		const char FUNCTION_START = '(';
		const char FUNCTION_END = ')';
		const char PROPERTIES_SEPARATOR = ',';
		const char FIELD_SEPARATOR = '.';
		const char SPACE = ' ';

		Stack<string> prefixes { get; set; }
		List<string> analyseResult { get; set; }
		string buffer { get; set; }

		public FieldExpansionHelper()
			: this(new Stack<string>()) { }

		FieldExpansionHelper(Stack<string> prefixes)
		{
			this.prefixes = prefixes;
			this.buffer = string.Empty;
			this.analyseResult = new List<string>();
		}

		void EmptyBuffer()
		{
			if (!string.IsNullOrWhiteSpace(buffer))
			{
				analyseResult.Add(string.Join(string.Empty, prefixes.Reverse()) + buffer);
				buffer = string.Empty;
			}
		}

		void FeedBuffer(char c) { buffer += c; }

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

