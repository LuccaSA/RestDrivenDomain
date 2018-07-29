using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using NExtends.Primitives.DateTimes;
using RDD.Domain.Helpers;
using RDD.Domain.Models;
using RDD.Domain.Models.Querying;
using RDD.Infra.Web.Models;

namespace RDD.Web.Querying
{
    public class WebFilterParser : IWebFilterParser
    {
        private readonly QueryTokens _queryTokens;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private static readonly Dictionary<string, WebFilterOperand> Operands = new Dictionary<string, WebFilterOperand>
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

        public WebFilterParser(QueryTokens queryTokens, IHttpContextAccessor httpContextAccessor)
        {
            _queryTokens = queryTokens;
            _httpContextAccessor = httpContextAccessor;
        }

        public IEnumerable<WebFilter<TEntity>> ParseWebFilters<TEntity>()
            where TEntity : class
        {
            var service = new DeserializationService();

            foreach (var kv in _httpContextAccessor.HttpContext.Request.Query)
            {
                if (string.IsNullOrEmpty(kv.Key))
                {
                    continue;
                }

                var key = kv.Key.Split('.')[0];

                if (_queryTokens.IsTokenReserved(kv.Key))
                {
                    continue;
                }

                foreach (var stringValue in kv.Value)
                {
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

                    yield return new WebFilter<TEntity>(property, operand, values);
                }
            }
        }
    }
}