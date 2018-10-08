using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Rdd.Domain;
using Rdd.Domain.Helpers;
using Rdd.Domain.Models.Querying;
using Rdd.Infra.Helpers;
using Rdd.Infra.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rdd.Web.Querying
{
    public class QueryParser<TEntity> : IQueryParser<TEntity>
        where TEntity : class
    {
        protected IWebFilterConverter<TEntity> WebFilterConverter { get; private set; }
        protected IPagingParser PagingParser { get; private set; }
        protected IFilterParser FilterParser { get; private set; }
        protected IFieldsParser FieldsParser { get; private set; }
        protected IOrderByParser OrderByParser { get; private set; }

        protected HashSet<string> IgnoredFilters { get; set; }

        public QueryParser(IWebFilterConverter<TEntity> webFilterConverter, IPagingParser pagingParser, IFilterParser filterParser, IFieldsParser fieldsParser, IOrderByParser orderByParser)
        {
            WebFilterConverter = webFilterConverter ?? throw new ArgumentNullException(nameof(webFilterConverter));
            PagingParser = pagingParser ?? throw new ArgumentNullException(nameof(pagingParser));
            FilterParser = filterParser ?? throw new ArgumentNullException(nameof(filterParser));
            FieldsParser = fieldsParser ?? throw new ArgumentNullException(nameof(fieldsParser));
            OrderByParser = orderByParser ?? throw new ArgumentNullException(nameof(orderByParser));

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
            var query = new WebQuery<TEntity>
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
                query.Fields = FieldsParser.GetDeFaultFields<TEntity>(isCollectionCall);
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
                        query.Fields = FieldsParser.Parse<TEntity>(value);
                        break;

                    case Reserved.orderby:
                        query.OrderBys = OrderByParser.Parse<TEntity>(value);
                        break;

                    case Reserved.paging:
                        query.Page = PagingParser.Parse(value);
                        break;
                }
            }
            else
            {
                filters.Add(FilterParser.Parse<TEntity>(key, value));
            }
        }
    }
}