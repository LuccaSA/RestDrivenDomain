using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using NExtends.Primitives.DateTimes;
using RDD.Domain;
using RDD.Domain.Exceptions;
using RDD.Domain.Helpers;
using RDD.Domain.Helpers.Expressions;
using RDD.Domain.Models;
using RDD.Domain.Models.Querying;
using RDD.Infra.Helpers;
using RDD.Infra.Web.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RDD.Web.Querying
{
    public class QueryParser
    {
        protected static readonly IReadOnlyDictionary<string, WebFilterOperand> Operands = new Dictionary<string, WebFilterOperand>(StringComparer.OrdinalIgnoreCase)
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

        protected static readonly IReadOnlyDictionary<string, SortDirection> DirectionsByKeyword = new Dictionary<string, SortDirection>(StringComparer.OrdinalIgnoreCase)
        {
            { "asc", SortDirection.Ascending },
            { "desc", SortDirection.Descending }
        };

        protected QueryParser() { }
    }

    public class QueryParser<TEntity> : QueryParser, IQueryParser<TEntity>
        where TEntity : class
    {
        protected IStringConverter StringConverter { get; private set; }
        protected IExpressionParser ExpressionParser { get; private set; }
        protected IWebFilterConverter<TEntity> WebFilterConverter { get; private set; }

        protected HashSet<string> IgnoredFilters { get; set; }

        public QueryParser(IStringConverter stringConverter, IExpressionParser expressionParser, IWebFilterConverter<TEntity> webFilterConverter)
        {
            StringConverter = stringConverter ?? throw new ArgumentNullException(nameof(stringConverter));
            ExpressionParser = expressionParser ?? throw new ArgumentNullException(nameof(expressionParser));
            WebFilterConverter = webFilterConverter ?? throw new ArgumentNullException(nameof(webFilterConverter));

            IgnoredFilters = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        public void IgnoreFilters(params string[] filters)
        {
            foreach (var filter in filters)
            {
                IgnoredFilters.Add(filter);
            }
        }

        public virtual Query<TEntity> Parse(HttpContext context, bool isCollectionCall)
            => Parse(GetVerb(context), context.Request.Query, isCollectionCall);

        public virtual Query<TEntity> Parse(HttpVerbs verb, IEnumerable<KeyValuePair<string, StringValues>> parameters, bool isCollectionCall)
        {
            var filters = new List<WebFilter<TEntity>>();
            var query = new Query<TEntity>
            {
                Fields = null,
                Verb = verb
            };

            foreach (var parameter in parameters.Where(v => !string.IsNullOrEmpty(v.Key) && !IgnoredFilters.Contains(v.Key)))
            {
                ParseParameter(parameter.Key, parameter.Value, query, filters);
            }

            if (query.Fields == null)
            {
                query.Fields = GetDeFaultFields(isCollectionCall);
            }
            else if (query.Fields.Contains((ISelection c) => c.Count))
            {
                query.Options.NeedCount = true;
                query.Options.NeedEnumeration = query.Fields.Children.Count() != 1;
            }

            query.Filter = WebFilterConverter.ToExpression(filters);

            return query;
        }

        protected virtual HttpVerbs GetVerb(HttpContext context)
        {
            if (HttpMethods.IsGet(context.Request.Method)) return HttpVerbs.Get;
            if (HttpMethods.IsPost(context.Request.Method)) return HttpVerbs.Post;
            if (HttpMethods.IsPut(context.Request.Method)) return HttpVerbs.Put;
            if (HttpMethods.IsDelete(context.Request.Method)) return HttpVerbs.Delete;
            return HttpVerbs.None;
        }

        protected virtual void ParseParameter(string key, string value, Query<TEntity> query, List<WebFilter<TEntity>> filters)
        {
            if (Enum.TryParse<Reserved>(key, true, out var reserved))
            {
                switch (reserved)
                {
                    case Reserved.fields:
                        query.Fields = ParseFields(value);
                        break;

                    case Reserved.orderby:
                        query.OrderBys = ParseOrderBy(value);
                        break;

                    case Reserved.paging:
                        query.Page = ParsePaging(value);
                        break;
                }
            }
            else
            {
                filters.Add(ParseFilter(key, value));
            }
        }

        protected virtual List<OrderBy<TEntity>> ParseOrderBy(string value)
        {
            var orders = value.Split(',');
            if (orders.Length % 2 != 0)
            {
                throw new BadRequestException("Correct order format is `orderby=(property,[asc|desc])*`");
            }

            var result = new List<OrderBy<TEntity>>();
            for (var i = 0; i < orders.Length; i += 2)
            {
                if (!DirectionsByKeyword.ContainsKey(orders[i + 1]))
                {
                    throw new BadRequestException("Correct order format is `orderby=(property,[asc|desc])*`");
                }

                var expression = ExpressionParser.Parse<TEntity>(orders[i]);
                result.Add(new OrderBy<TEntity>(expression, DirectionsByKeyword[orders[i + 1]]));
            }

            return result;
        }

        protected virtual IExpressionTree<TEntity> GetDeFaultFields(bool isCollectionCall)
        {
            if (!isCollectionCall)
            {
                return ParseFields(string.Join(",", typeof(TEntity).GetProperties().Select(p => p.Name)));
            }
            else
            {
                return new ExpressionTree<TEntity>();
            }
        }

        protected virtual IExpressionTree<TEntity> ParseFields(string fields) => ExpressionParser.ParseTree<TEntity>(fields);

        protected virtual WebFilter<TEntity> ParseFilter(string key, string value)
        {
            var parts = value.Split(',').ToList();
            var operand = ExtractFilterOperand(parts);

            var chain = ExpressionParser.ParseChain<TEntity>(key);
            var values = ConvertFilterValues(operand, chain, parts);

            return new WebFilter<TEntity>(chain, operand, values);
        }

        protected virtual WebFilterOperand ExtractFilterOperand(List<string> parts)
        {
            if (parts[0] != null && Operands.ContainsKey(parts[0]))
            {
                var result = Operands[parts[0]];
                parts.RemoveAt(0);
                return result;
            }
            return WebFilterOperand.Equals;
        }

        protected virtual IList ConvertFilterValues(WebFilterOperand operand, IExpression expression, IEnumerable<string> parts)
        {
            var values = StringConverter.ConvertValues(expression, parts);

            if (operand is WebFilterOperand.Between && values.Count == 2 && values[0] is DateTime? && values[1] is DateTime?)
            {
                values = new List<Period> { new Period((DateTime)values[0], ((DateTime)values[1]).ToMidnightTimeIfEmpty()) };
            }

            return values;
        }

        protected virtual Page ParsePaging(string input)
        {
            var elements = input.Split(',');
            if (elements.Length != 2 || !int.TryParse(elements[0], out var offset) || !int.TryParse(elements[1], out var limit))
            {
                throw new BadRequestException($"Correct paging format is 'paging=offset,count'.");
            }

            return new WebPage(offset, limit);
        }
    }
}