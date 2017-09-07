using Newtonsoft.Json.Linq;
using NExtends.Primitives;
using RDD.Domain;
using RDD.Domain.Helpers;
using RDD.Domain.Models;
using RDD.Domain.Models.Querying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RDD.Web.Querying
{
    public class FiltersParser<TEntity>
		where TEntity : class, IEntityBase
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

		public List<Filter<TEntity>> Parse(Dictionary<string, string> input)
		{
			var reserved = Enum.GetNames(typeof(Reserved)).ToLower();

			var keys = input.Keys.Where(k => !reserved.Contains(k.Split('.')[0]));

			return Parse(input, keys);
		}
		private static List<Filter<TEntity>> Parse(Dictionary<string, string> input, IEnumerable<string> keys)
		{
			var list = new List<Filter<TEntity>>();

			foreach (var key in keys)
			{
				var stringValue = input[key];

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
				if (data[0].value != null && Operands.ContainsKey(data[0].value))
				{
					type = Operands[data[0].value];
					data.subs.Remove("0"); //On vire l'entrée qui correspondait en fait au mot clé
				}

				var service = new SerializationService();
				var values = service.ConvertWhereValues(data.values.Select(p => p.value).ToList(), typeof(TEntity), key);

				//cas spécial pour between et until
				if (type == FilterOperand.Between)
				{
					//cas général : c'est une période, mais pour un department on peut avoir 2 decimals
					if ((values[0] as DateTime?) != null)
					{
						values = new List<Period> { new Period((DateTime)values[0], ((DateTime)values[1]).ToMidnightTimeIfEmpty()) };
					}
				}
				else if (type == FilterOperand.Until)
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

				var property = new PropertySelector<TEntity>();
				property.Parse(key);

				list.Add(new Filter<TEntity>(property, type, values));
			}

			return list;
		}
	}
}
