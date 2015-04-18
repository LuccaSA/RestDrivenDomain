using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Infra.Models.Querying
{
	public class Field
	{
		public enum Reserved
		{
			[Description("Champ permettant de demander des propriétés de la collection elle-même plutôt que des propriétés des entités")]
			collection,
		}

		public string name { get; set; }
		public Dictionary<string, Field> subs { get; private set; }

		public Field(string name, Dictionary<string, Field> subs)
		{
			this.name = name;
			this.subs = subs ?? new Dictionary<string, Field>(StringComparer.OrdinalIgnoreCase); //subs est jamais null
		}

		public static Field Parse(string fields)
		{
			fields = fields ?? "";
			fields = fields.Replace(", ", ",");
			return Parse("this", ExpansionHelper.Expand(fields));
		}
		public static Field Parse(List<string> fields)
		{
			return Parse("this", fields ?? new List<string>());
		}

		static Field Parse(string name, IEnumerable<string> fields)
		{
			var result = fields.ToLowerDotDictionary();

			return new Field(name, result.ToDictionary(p => p.Key, p => Parse(p.Key, p.Value == null ? new List<string>() : p.Value), StringComparer.OrdinalIgnoreCase));
		}

		/// <summary>
		/// Implémentation des [], comme si Field était un Dictionary !!
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public Field this[string key]
		{
			get { return subs[key]; }
			set { subs[key] = value; }
		}

		public ICollection<string> Keys { get { return subs == null ? null : subs.Keys; } }

		public bool ContainsKey(string key)
		{
			return subs.ContainsKey(key);
		}
		public bool ContainsKey(Enum key)
		{
			return subs.ContainsKey(key);
		}
		public bool ContainsKeys(params string[] keys)
		{
			return keys.All(key => this.subs.ContainsKey(key));
		}
		public int Count { get { return subs.Count; } }
		public bool Any() { return subs.Any(); }

		internal class ExpansionHelper
		{
			const char MULTISELECT_START = '[';
			const char MULTISELECT_END = ']';
			const char FUNCTION_START = '(';
			const char FUNCTION_END = ')';
			const char PROPERTIES_SEPARATOR = ',';
			const char FIELD_SEPARATOR = '.';

			public static List<string> Expand(string input) { return new ExpansionHelper(new Stack<string>()).Expanse(input); }

			Stack<string> prefixes { get; set; }
			List<string> analyseResult { get; set; }
			string buffer { get; set; }

			ExpansionHelper(Stack<string> prefixes)
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

			List<string> Expanse(string input)
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
								if (level < 0) { throw new FormatException("Badly formatted field"); }
								analyseResult.AddRange(new ExpansionHelper(prefixes).Expanse(buffer));
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
						default:
							FeedBuffer(character);
							break;
					}
				}

				if (level != 0)
				{
					throw new FormatException("Badly formatted field");
				}
				EmptyBuffer();
				return analyseResult;
			}
		}
	}
}