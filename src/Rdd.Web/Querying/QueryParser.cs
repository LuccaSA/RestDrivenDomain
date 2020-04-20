using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Rdd.Domain;
using Rdd.Domain.Helpers;
using Rdd.Domain.Models.Querying;
using Rdd.Infra.Helpers;
using System;

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
        private readonly ITypeFilterParser<TEntity> _typeFilterParser;

        public QueryParser(IWebFilterConverter<TEntity> webFilterConverter, IPagingParser pagingParser, IFilterParser filterParser, IFieldsParser fieldsParser, IOrderByParser orderByParser, ITypeFilterParser<TEntity> typeFilterParser)
        {
            _webFilterConverter = webFilterConverter ?? throw new ArgumentNullException(nameof(webFilterConverter));
            _pagingParser = pagingParser ?? throw new ArgumentNullException(nameof(pagingParser));
            _filterParser = filterParser ?? throw new ArgumentNullException(nameof(filterParser));
            _fieldsParser = fieldsParser ?? throw new ArgumentNullException(nameof(fieldsParser));
            _orderByParser = orderByParser ?? throw new ArgumentNullException(nameof(orderByParser));
            _typeFilterParser = typeFilterParser ?? throw new ArgumentNullException(nameof(typeFilterParser));
        }

        public virtual Query<TEntity> Parse(HttpRequest request, bool isCollectionCall)
            => Parse(request, null, isCollectionCall);

        public virtual Query<TEntity> Parse(HttpRequest request, ActionDescriptor action, bool isCollectionCall)
        {
            var query = new Query<TEntity>
            (
                 _fieldsParser.Parse<TEntity>(request, isCollectionCall),
                 _orderByParser.Parse<TEntity>(request),
                 _pagingParser.Parse(request),
                 _filterParser.Parse(request, action, _webFilterConverter),
                GetVerb(request)
            )
            {
                TypeFilter = _typeFilterParser.Parse(request)
            };

            if (query.Fields.Contains((ISelection c) => c.Count))
            {
                query.Options.NeedsCount = true;
                query.Options.NeedsEnumeration = query.Fields.Children.Count != 1;
            }

            if (query.Verb == HttpVerbs.Get)
            {
                query.Options.NeedsDataTracking = false;
            }

            return query;
        }

        protected virtual HttpVerbs GetVerb(HttpRequest request)
        {
            if (HttpMethods.IsGet(request.Method))
            {
                return HttpVerbs.Get;
            }

            if (HttpMethods.IsPost(request.Method))
            {
                return HttpVerbs.Post;
            }

            if (HttpMethods.IsPut(request.Method))
            {
                return HttpVerbs.Put;
            }

            if (HttpMethods.IsDelete(request.Method))
            {
                return HttpVerbs.Delete;
            }

            return HttpVerbs.None;
        }
    }
}