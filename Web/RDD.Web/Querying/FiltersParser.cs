using NExtends.Primitives.DateTimes;
using NExtends.Primitives.Strings;
using RDD.Domain;
using RDD.Domain.Helpers;
using RDD.Domain.Models;
using RDD.Domain.Models.Querying;
using System;
using System.Collections.Generic;
using System.Linq;

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
            var service = new SerializationService();

            foreach (string key in keys)
            {
                string stringValue = input[key];
                var parts = stringValue.Split(',').ToList();

                var operand = FilterOperand.Equals;

                //si la premier attribut n'est pas un mot clé, on a un equals (mis par défaut plus haut) ex : id=20,30 ; sinon, on le reconnait dans le dico
                //PS : dans le cas où data contient du JSON, alors .value peut être null
                if (parts[0] != null && _operands.ContainsKey(parts[0]))
                {
                    operand = _operands[parts[0]];
                    parts.RemoveAt(0); //On vire l'entrée qui correspondait en fait au mot clé
                }

                var values = service.ConvertWhereValues(parts, typeof(TEntity), key);

                //cas spécial pour between (filtre sur un department => decimals, != datetime)
                if (operand == FilterOperand.Between && values.Count == 2 && (values[0] as DateTime?) != null)
                {
                    values = new List<Period> { new Period((DateTime)values[0], ((DateTime)values[1]).ToMidnightTimeIfEmpty()) };
                }

                var property = new PropertySelector<TEntity>();
                property.Parse(key);

                list.Add(new Filter<TEntity>(property, operand, values));
            }

            return list;
        }
    }
}