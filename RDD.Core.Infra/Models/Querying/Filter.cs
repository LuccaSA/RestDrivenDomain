using Newtonsoft.Json.Linq;
using RDD.Infra.Helpers;
using RDD.Infra.Models.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Infra.Models.Querying
{
	public class Filter
	{
		public static Dictionary<string, FilterOperand> Operands = new Dictionary<string, FilterOperand>()
		{
			{"between", FilterOperand.Between},
			{"equals", FilterOperand.Equals},
			{"notequal", FilterOperand.NotEqual},
			{"like", FilterOperand.Like},
			{"since", FilterOperand.Since},
			{"starts", FilterOperand.Starts},
			{"until", FilterOperand.Until},
			{"greaterthan", FilterOperand.GreaterThan},
			{"greaterthanorequal", FilterOperand.GreaterThanOrEqual},
			{"lessthan", FilterOperand.LessThan},
			{"lessthanorequal", FilterOperand.LessThanOrEqual}
		};

		public string Field { get; set; }
		public FilterOperand Type { get; set; }
		public List<object> Values { get; set; }

		public static List<Filter> ParseOperations<T>(PostedData datas)
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
		public static List<Filter> Parse<T>(PostedData datas)
		{
			var reserved = Enum.GetNames(typeof(Reserved)).ToLower();

			var keys = datas.Keys.Where(k => !reserved.Contains(k));

			return Parse<T>(datas, keys);
		}
		private static List<Filter> Parse<T>(PostedData datas, IEnumerable<string> keys)
		{
			var list = new List<Filter>();

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

				var type = FilterOperand.Equals;

				//si la premier attribut n'est pas un mot clé, on a un equals (mis par défaut plus haut) ex : id=20,30 ; sinon, on le reconnait dans le dico
				//PS : dans le cas où data contient du JSON, alors .value peut être null
				if (data[0].value != null && Filter.Operands.ContainsKey(data[0].value))
				{
					type = Filter.Operands[data[0].value];
					data.subs.Remove("0"); //On vire l'entrée qui correspondait en fait au mot clé
				}

				var helper = new PostedDataHelper();
				List<object> values = data.values.Select(v => helper.TryConvert(v, typeof(T), key)).ToList();

				//cas spécial pour between et until
				if (type == FilterOperand.Between)
				{
					//cas général : c'est une période, mais pour un department on peut avoir 2 decimals
					if ((values[0] as DateTime?) != null)
					{
						values = new List<object>() { new Period((DateTime)values[0], ((DateTime)values[1]).ToMidnightTimeIfEmpty()) };
					}
				}
				else if (type == FilterOperand.Until)
				{
					//cas général : c'est une date, mais pour un leave on peut avoir un int
					if ((values[0] as DateTime?) != null)
					{
						values = new List<object>() { ((DateTime)values[0]).ToMidnightTimeIfEmpty() };
					}
					else if ((values[0] as int?) != null)
					{
						values = new List<object>() { (int)values[0] };
					}
				}


				var where = new Filter { Type = type, Field = key, Values = values };

				list.Add(where);
			}

			return list;
		}
	}
}
