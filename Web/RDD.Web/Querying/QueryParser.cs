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
using Microsoft.Extensions.Options;
using Rdd.Web.Helpers;

namespace Rdd.Web.Querying
{
    public class QueryParser<TEntity> : IQueryParser<TEntity>
        where TEntity : class
    {
        private readonly IWebFilterConverter<TEntity> _webFilterConverter;
        private readonly IPagingParser _pagingParser;
        private readonly IFilterParser _filterParser;
        private readonly IFieldsParser _fieldsParser;
        private readonly IOrderByParser _orderByParser;
        private readonly IOptions<RddOptions> _rddOptions;

        protected HashSet<string> IgnoredFilters { get; set; }

        public QueryParser(IWebFilterConverter<TEntity> webFilterConverter, IPagingParser pagingParser, IFilterParser filterParser, IFieldsParser fieldsParser, IOrderByParser orderByParser, IOptions<RddOptions> rddOptions)
        {
            _webFilterConverter = webFilterConverter ?? throw new ArgumentNullException(nameof(webFilterConverter));
            _pagingParser = pagingParser ?? throw new ArgumentNullException(nameof(pagingParser));
            _filterParser = filterParser ?? throw new ArgumentNullException(nameof(filterParser));
            _fieldsParser = fieldsParser ?? throw new ArgumentNullException(nameof(fieldsParser));
            _orderByParser = orderByParser ?? throw new ArgumentNullException(nameof(orderByParser));
            _rddOptions = rddOptions;

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
                query.Fields = _fieldsParser.GetDeFaultFields<TEntity>(isCollectionCall);
            }
            else if (query.Fields.Contains((ISelection c) => c.Count))
            {
                query.Options.NeedCount = true;
                query.Options.NeedEnumeration = query.Fields.Children.Count() != 1;
            }

            if (query.Page == Page.Unlimited)
            {
                query.Page = _rddOptions.Value.DefaultPage;
            }

            query.Filter = _webFilterConverter.ToExpression(filters);

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
                        query.Fields = _fieldsParser.Parse<TEntity>(value);
                        break;

                    case Reserved.orderby:
                        query.OrderBys = _orderByParser.Parse<TEntity>(value);
                        break;

                    case Reserved.paging:
                        query.Page = _pagingParser.Parse(value);
                        break;
                }
            }
            else
            {
                filters.Add(_filterParser.Parse<TEntity>(key, value));
            }
        }
    }
}