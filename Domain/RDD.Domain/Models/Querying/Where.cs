using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.Linq.Expressions;
using LinqKit;
using Newtonsoft.Json.Linq;
using System.Collections;
using NExtends.Primitives;

namespace RDD.Domain.Models.Querying
{
	/// <summary>
	/// Opérateurs de comparaison d'expressions
	/// </summary>
	/// <see cref="https://msdn.microsoft.com/en-us/library/bb361179%28v=vs.110%29.aspx"/>
	public enum WhereOperand
	{
		[Description("Filter on Equality. usage : users?name=equals,bob  or users?name=bob or users?name=bob,arnold")]
		Equals,
		[Description("Filter on Unequality. usage : users?name=notequal,bob")]
		NotEqual,
		[Description("Filter on data starting with the following parameter. usage : users?name=starts,a")]
		Starts,
		[Description("Filter on data containing the following text. usage : users?name=like,a")]
		Like,
		[Description("Filter on date for which value is between following parameters. usage : users?dtContractEnd=between,2013-01-01,2014-01-01")]
		Between,
		/// <summary>
		/// Equivalent à l'opérateur 'LessThanOrEqual', mais exclusif aux dates
		/// </summary>
		[Description("Filter on date for which value is inferior to the following parameter. usage : users?dtContractEnd=until,today")]
		Until,
		/// <summary>
		/// Equivalent à l'opérateur 'GreaterThanOrEqual', mais exclusif aux dates
		/// </summary>
		[Description("Filter on date for which value is superior to the following parameter. usage : users?dtContractEnd=since,today")]
		Since,
		/// <summary>
		/// Filter on expression strictly superior to the second operand expression
		/// </summary>
		/// <example>files?size=greaterthan,50</example>
		[Description("Filter on expression strictly superior to the second operand expression. usage : files?size=greaterthan,50")]
		GreaterThan,
		/// <summary>
		/// Filter on expression strictly superior or equal to the second operand expression
		/// </summary>
		/// <example>files?size=greaterthanorequal,50</example>
		[Description("Filter on expression superior or equal to the second operand expression. usage : files?size=greaterthanorequal,50")]
		GreaterThanOrEqual,
		/// <summary>
		/// Filter on expression strictly inferior to the second operand expression
		/// </summary>
		/// <example>files?size=lessthan,50</example>
		[Description("Filter on expression strictly inferior to the second operand expression. usage : files?size=lessthan,50")]
		LessThan,
		/// <summary>
		/// Filter on expression inferior or equal to the second operand expression
		/// </summary>
		/// <example>files?size=lessthanorequal,50</example>
		[Description("Filter on expression inferior or equal to the second operand expression. usage : files?size=lessthanorequal,50")]
		LessThanOrEqual
	}

	public class Where
	{
		public static Dictionary<string, WhereOperand> Operands = new Dictionary<string, WhereOperand>()
		{
			{"between", WhereOperand.Between},
			{"equals", WhereOperand.Equals},
			{"notequal", WhereOperand.NotEqual},
			{"like", WhereOperand.Like},
			{"since", WhereOperand.Since},
			{"starts", WhereOperand.Starts},
			{"until", WhereOperand.Until},
			{"greaterthan", WhereOperand.GreaterThan},
			{"greaterthanorequal", WhereOperand.GreaterThanOrEqual},
			{"lessthan", WhereOperand.LessThan},
			{"lessthanorequal", WhereOperand.LessThanOrEqual}
		};

		public string Field { get; set; }
		public WhereOperand Type { get; set; }
		public IList Values { get; set; }

		internal static List<Where> ParseOperations<T>(PostedData datas)
		{
			var prefix = "operations.";
			var keys = datas.Keys.Where(k => k.StartsWith(prefix));

			var result = Parse<T>(datas, keys);

			foreach (var where in result)
			{
				where.Field = where.Field.Substring(prefix.Length); //operations.name=toto => name=toto
			}

			return result;
		}
		public static List<Where> Parse<T>(PostedData datas)
		{
			var reserved = Enum.GetNames(typeof(Reserved)).ToLower();

			var keys = datas.Keys.Where(k => !reserved.Contains(k.Split('.')[0]));

			return Parse<T>(datas, keys);
		}
		private static List<Where> Parse<T>(PostedData datas, IEnumerable<string> keys)
		{
			var list = new List<Where>();

			foreach (var key in keys)
			{
				var stringValue = datas[key].value;

				PostedData data;
				bool isJsonObject = false; //Par défaut on considère que ce sont des types simples séparés par des ,

				//Mais ça peut être 1 ou plusieurs objets JSON séparés par des ,
				if (stringValue.StartsWith("{"))
				{
					isJsonObject = true;
				}

				if (isJsonObject)
				{
					data = PostedData.ParseJSONArray("[" + stringValue + "]");
				}
				else
				{
					data = PostedData.ParseJSONArray(JArray.Parse("[" + String.Join(", ", stringValue.Split(',').Select(p => p.ToJSON()).ToArray()) + "]"));
				}

				var type = WhereOperand.Equals;

				//si la premier attribut n'est pas un mot clé, on a un equals (mis par défaut plus haut) ex : id=20,30 ; sinon, on le reconnait dans le dico
				//PS : dans le cas où data contient du JSON, alors .value peut être null
				if (data[0].value != null && Where.Operands.ContainsKey(data[0].value))
				{
					type = Where.Operands[data[0].value];
					data.subs.Remove("0"); //On vire l'entrée qui correspondait en fait au mot clé
				}

				var service = new SerializationService();
				var values = service.ConvertWhereValues(data.values.Select(p => p.value).ToList(), typeof(T), key);

				//cas spécial pour between et until
				if (type == WhereOperand.Between)
				{
					//cas général : c'est une période, mais pour un department on peut avoir 2 decimals
					if ((values[0] as DateTime?) != null)
					{
						values = new List<Period> { new Period((DateTime)values[0], ((DateTime)values[1]).ToMidnightTimeIfEmpty()) };
					}
				}
				else if (type == WhereOperand.Until)
				{
					//cas général : c'est une date, mais pour un leave on peut avoir un int
					if ((values[0] as DateTime?) != null)
					{
						values = new List<DateTime> { ((DateTime)values[0]).ToMidnightTimeIfEmpty() };
					}
					else if ((values[0] as int?) != null)
					{
						values = new List<int> { (int)values[0] };
					}
				}
				list.Add(new Where { Type = type, Field = key, Values = values });
			}

			return list;
		}
	}
}