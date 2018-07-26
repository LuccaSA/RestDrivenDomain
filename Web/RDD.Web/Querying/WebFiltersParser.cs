using NExtends.Primitives.DateTimes;
using NExtends.Primitives.Strings;
using RDD.Domain;
using RDD.Domain.Helpers;
using RDD.Domain.Models;
using RDD.Domain.Models.Querying;
using RDD.Infra.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using RDD.Web.Middleware;

namespace RDD.Web.Querying
{
    public class WebFiltersParser
    {
        protected WebFiltersParser() { }

        protected static readonly Dictionary<string, WebFilterOperand> Operands = new Dictionary<string, WebFilterOperand>
        {
            {"between", WebFilterOperand.Between},
            {"equals", WebFilterOperand.Equals},
            {"notequal", WebFilterOperand.NotEqual},
            {"like", WebFilterOperand.Like},
            {"since", WebFilterOperand.Since},
            {"starts", WebFilterOperand.Starts},
            {"until", WebFilterOperand.Until},
            {"greaterthan", WebFilterOperand.GreaterThan},
            {"greaterthanorequal", WebFilterOperand.GreaterThanOrEqual},
            {"lessthan", WebFilterOperand.LessThan},
            {"lessthanorequal", WebFilterOperand.LessThanOrEqual}
        };
    }

    public class WebFiltersParser<TEntity> : WebFiltersParser
        where TEntity : class
    {
        protected WebFiltersParser() { }

        

        public static List<WebFilter<TEntity>> Parse(Dictionary<string, string> input)
        { 
            IEnumerable<string> keys = input.Keys.Where(k => !QueryTokens.Reserved.Contains(k.Split('.')[0]));

            return Parse(input, keys);
        }

        private static List<WebFilter<TEntity>> Parse(Dictionary<string, string> input, IEnumerable<string> keys)
        {
            var list = new List<WebFilter<TEntity>>();
            var service = new SerializationService();

            foreach (string key in keys)
            {
                string stringValue = input[key];
                var parts = stringValue.Split(',').ToList();

                var operand = WebFilterOperand.Equals;

                //si la premier attribut n'est pas un mot clé, on a un equals (mis par défaut plus haut) ex : id=20,30 ; sinon, on le reconnait dans le dico
                //PS : dans le cas où data contient du JSON, alors .value peut être null
                if (parts[0] != null && Operands.ContainsKey(parts[0]))
                {
                    operand = Operands[parts[0]];
                    parts.RemoveAt(0); //On vire l'entrée qui correspondait en fait au mot clé
                }

                var values = service.ConvertWhereValues(parts, typeof(TEntity), key);

                //cas spécial pour between (filtre sur un department => decimals, != datetime)
                if (operand is WebFilterOperand.Between && values.Count == 2 && values[0] is DateTime?)
                {
                    values = new List<Period> { new Period((DateTime)values[0], ((DateTime)values[1]).ToMidnightTimeIfEmpty()) };
                }

                var property = new PropertySelector<TEntity>();
                property.Parse(key);

                list.Add(new WebFilter<TEntity>(property, operand, values));
            }

            return list;
        }
    }
}