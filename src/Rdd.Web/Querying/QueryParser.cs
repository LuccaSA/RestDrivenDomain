using Microsoft.AspNetCore.Http;
using Rdd.Domain;
using Rdd.Domain.Helpers;
using Rdd.Domain.Models.Querying;
using Rdd.Infra.Helpers;
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

        public virtual Query<TEntity> Parse(HttpRequest request, bool isCollectionCall)
        { 
            var query = new Query<TEntity>
            {
                Fields = null,
                Verb = GetVerb(request)
            };

            if (request.Query.TryGetValue(Reserved.Fields, out var fieldValue))
            {
                query.Fields = _fieldsParser.Parse<TEntity>(fieldValue);
                if (query.Fields.Contains((ISelection c) => c.Count))
                {
                    query.Options.NeedCount = true;
                    query.Options.NeedEnumeration = query.Fields.Children.Count() != 1;
                }
            }
            else
            {
                query.Fields = _fieldsParser.GetDeFaultFields<TEntity>(isCollectionCall);
            }

            if (request.Query.TryGetValue(Reserved.Orderby, out var orderByValue))
            {
                query.OrderBys = _orderByParser.Parse<TEntity>(orderByValue);
            }

            if (request.Query.TryGetValue(Reserved.Paging, out var pageValue))
            {
                query.Page = _pagingParser.Parse(pageValue);
            }
            else
            {
                query.Page = _rddOptions.Value.DefaultPage;
            }

            var filters = request.Query
                .Where(kv => !Reserved.Keywords.Contains(kv.Key))
                .Select(kv => _filterParser.Parse<TEntity>(kv.Key, kv.Value));
             
            query.Filter = _webFilterConverter.ToExpression(filters);

            return query;
        }

        protected virtual HttpVerbs GetVerb(HttpRequest request)
        {
            if (HttpMethods.IsGet(request.Method)) return HttpVerbs.Get;
            if (HttpMethods.IsPost(request.Method)) return HttpVerbs.Post;
            if (HttpMethods.IsPut(request.Method)) return HttpVerbs.Put;
            if (HttpMethods.IsDelete(request.Method)) return HttpVerbs.Delete;
            return HttpVerbs.None;
        }
    }
}