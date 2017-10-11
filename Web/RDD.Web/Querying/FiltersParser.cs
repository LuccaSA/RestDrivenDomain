using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using NExtends.Primitives;
using RDD.Domain;
using RDD.Domain.Helpers;
using RDD.Domain.Models;
using RDD.Domain.Models.Querying;

namespace RDD.Web.Querying
{
    public class FiltersParser<TEntity>
        where TEntity : class, IEntityBase
    {
        private static readonly Dictionary<string, FilterOperand> _operands = new Dictionary<string, FilterOperand>
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
            string[] reserved = Enum.GetNames(typeof(Reserved)).ToLower();

            IEnumerable<string> keys = input.Keys.Where(k => !reserved.Contains(k.Split('.')[0]));

            return Parse(input, keys);
        }

        private static List<Filter<TEntity>> Parse(Dictionary<string, string> input, IEnumerable<string> keys)
        {
            var list = new List<Filter<TEntity>>();

            foreach (string key in keys)
            {
                string stringValue = input[key];

                PostedData data;
                //Par défaut on considère que ce sont des types simples séparés par des ,
                //Mais ça peut être 1 ou plusieurs objets JSON séparés par des ,
                bool isJsonObject = stringValue.StartsWith("{");

                if (isJsonObject)
                {
                    data = PostedData.ParseJsonArray("[" + stringValue + "]");
                }
                else
                {
                    data = PostedData.ParseJsonArray(JArray.Parse("[" + string.Join(", ", stringValue.Split(',').Select(p => p.ToJSON()).ToArray()) + "]"));
                }

                var type = FilterOperand.Equals;

                //si la premier attribut n'est pas un mot clé, on a un equals (mis par défaut plus haut) ex : id=20,30 ; sinon, on le reconnait dans le dico
                //PS : dans le cas où data contient du JSON, alors .value peut être null
                if (data[0].Value != null && _operands.ContainsKey(data[0].Value))
                {
                    type = _operands[data[0].Value];
                    data.Subs.Remove("0"); //On vire l'entrée qui correspondait en fait au mot clé
                }

                var service = new SerializationService();
                IList values = service.ConvertWhereValues(data.Values.Select(p => p.Value).ToList(), typeof(TEntity), key);

                //cas spécial pour between et until
                if (type == FilterOperand.Between)
                {
                    //cas général : c'est une période, mais pour un department on peut avoir 2 decimals
                    if (values[0] is DateTime time)
                    {
                        values = new List<Period>
                        {
                            new Period(time, ((DateTime) values[1]).ToMidnightTimeIfEmpty())
                        };
                    }
                }
                else if (type == FilterOperand.Until)
                {
                    //cas général : c'est une date, mais pour un leave on peut avoir un int
                    if (values[0] is DateTime time)
                    {
                        values = new List<DateTime>
                        {
                            time.ToMidnightTimeIfEmpty()
                        };
                    }
                    else if (values[0] is int)
                    {
                        values = new List<int>
                        {
                            (int) values[0]
                        };
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