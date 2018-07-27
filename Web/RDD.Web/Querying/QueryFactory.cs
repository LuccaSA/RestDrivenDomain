using RDD.Domain;
using RDD.Domain.Models.Querying;
using RDD.Infra.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using NExtends.Primitives.DateTimes;
using RDD.Domain.Exceptions;
using RDD.Domain.Helpers;
using RDD.Domain.Models;

namespace RDD.Web.Querying
{
    /// <summary>
    /// Factory to generate Query<T/> items.
    /// </summary>
    public class QueryFactory
    {   
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly QueryTokens _queryTokens;
        private readonly QueryMetadata _queryMetadata;

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

        public QueryFactory(IHttpContextAccessor httpContextAccessor, QueryTokens queryTokens, QueryMetadata queryMetadata)
        {
            _httpContextAccessor = httpContextAccessor;
            _queryTokens = queryTokens;
            _queryMetadata = queryMetadata;
        }

        public Query<TEntity> FromWebContext<TEntity, TKey>()
            where TEntity : class, IPrimaryKey<TKey>
        {
            return new Query<TEntity>(
                new WebFiltersContainer<TEntity, TKey>(GetWebFilters<TEntity>(_httpContextAccessor.HttpContext.Request)),
                new Queue<OrderBy<TEntity>>(ParseOrderBys<TEntity>(_httpContextAccessor.HttpContext.Request)),
                ParseHeaders(_httpContextAccessor.HttpContext.Request),
                ParsePaging(_httpContextAccessor.HttpContext.Request),
                _queryMetadata
            );
        }

        public virtual IEnumerable<WebFilter<TEntity>> GetWebFilters<TEntity>(HttpRequest httpRequest)
            where TEntity : class
        { 
            var service = new SerializationService();

            foreach (var kv in httpRequest.Query)
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
                    //string stringValue = input[key];
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

        public virtual QueryPaging ParsePaging(HttpRequest httpRequest)
        {
            if (!_httpContextAccessor.HttpContext.Request.Query.TryGetValue(QueryTokens.Paging, out StringValues pagingValues))
            {
                return QueryPaging.Default;
            }

            if (pagingValues.Count > 1)
            {
                throw new BadRequestException("Does not respect \"limit=pageIndex,itemsPerPage\" format");
            }

            string paging = pagingValues.FirstOrDefault();
            if (String.IsNullOrEmpty(paging))
            {
                return QueryPaging.Default;
            }

            var qp = new QueryPaging();

            if (paging == "1") //...&paging=1 <=> &paging=0,100
            {
                qp.PageOffset = 0;
                qp.ItemPerPage = 100;
            }
            else //...&paging=x,y
            {
                var elements = paging.Split(',');

                if (elements.Length == 2)
                {
                    if (!Int32.TryParse(elements[0], out var pageOffset))
                    {
                        throw new BadRequestException($"PageIndex value {elements[0]} not in correct format");
                    }

                    if (!Int32.TryParse(elements[1], out var itemPerPage))
                    {
                        throw new BadRequestException($"ItemsPerPage value {elements[1]} not in correct format");
                    }

                    qp.PageOffset = pageOffset;
                    qp.ItemPerPage = 100;
                }
                else
                {
                    throw new BadRequestException($"{paging} does not respect limit=start,count format");
                }
            }

            return qp;
        }

        public static Headers ParseHeaders(HttpRequest httpRequest)
        {
            var headers = new Headers
            {
                RawHeaders = httpRequest.Headers
            };

            foreach (var element in headers.RawHeaders)
            {
                switch (element.Key)
                {
                    case "If-Unmodified-Since":
                        if (DateTime.TryParse(element.Value, out DateTime unModifiedSince))
                        {
                            headers.IfUnmodifiedSince = unModifiedSince;
                        }
                        break;
                    case "Authorization":
                        headers.Authorization = element.Value;
                        break;
                    case "Content-Type":
                        headers.ContentType = element.Value;
                        break;
                }
            }

            return headers;
        }

        public static IEnumerable<OrderBy<TEntity>> ParseOrderBys<TEntity>(HttpRequest httpRequest)
            where TEntity : class
        {
            if (!httpRequest.Query.TryGetValue(QueryTokens.OrderBy, out StringValues orderByValues))
            {
                yield break;
            }

            foreach (var clause in orderByValues)
            {
                var orders = clause.Split(',');
                var length = orders.Length;

                if (length % 2 != 0)
                {
                    throw new BadRequestException("Orders must contains order direction (asc or desc) for each field");
                }

                for (var i = 0; i < length; i += 2)
                {
                    var orderProperty = new PropertySelector<TEntity>();
                    orderProperty.Parse(orders[i].ToLower());

                    SortDirection direction;

                    if (string.Equals(orders[i + 1], "asc", StringComparison.InvariantCultureIgnoreCase))
                    {
                        direction = SortDirection.Ascending;
                    }
                    else if (string.Equals(orders[i + 1], "desc", StringComparison.InvariantCultureIgnoreCase))
                    {
                        direction = SortDirection.Ascending;
                    }
                    else
                    {
                        throw new BadRequestException("Order direction must match asc or desc");
                    }

                    yield return new OrderBy<TEntity>(orderProperty, direction); ;
                }
            }
        }

    }

}
