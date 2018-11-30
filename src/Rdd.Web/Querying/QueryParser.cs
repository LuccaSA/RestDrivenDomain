using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Rdd.Domain;
using Rdd.Domain.Helpers;
using Rdd.Domain.Models.Querying;
using System;

namespace Rdd.Web.Querying
{
    public class QueryParser<TEntity> : IQueryParser<TEntity>
        where TEntity : class
    {
        private readonly IPagingParser _pagingParser;
        private readonly IFilterParser<TEntity> _filterParser;
        private readonly IFieldsParser _fieldsParser;
        private readonly IOrderByParser<TEntity> _orderByParser;

        public QueryParser(IPagingParser pagingParser, IFilterParser<TEntity> filterParser, IFieldsParser fieldsParser, IOrderByParser<TEntity> orderByParser)
        {
            _pagingParser = pagingParser ?? throw new ArgumentNullException(nameof(pagingParser));
            _filterParser = filterParser ?? throw new ArgumentNullException(nameof(filterParser));
            _fieldsParser = fieldsParser ?? throw new ArgumentNullException(nameof(fieldsParser));
            _orderByParser = orderByParser ?? throw new ArgumentNullException(nameof(orderByParser));
        }

        public virtual Query<TEntity> Parse(HttpRequest request, bool isCollectionCall)
            => Parse(request, null, isCollectionCall);

        public virtual Query<TEntity> Parse(HttpRequest request, ActionDescriptor action, bool isCollectionCall)
        {
            var query = new Query<TEntity>
            (
                 _fieldsParser.Parse<TEntity>(request, isCollectionCall),
                 _orderByParser.Parse(request),
                 _pagingParser.Parse(request),
                 _filterParser.Parse(request, action),
                GetVerb(request)
            );

            if (query.Fields.Contains((ISelection c) => c.Count))
            {
                query.Options.NeedCount = true;
                query.Options.NeedEnumeration = query.Fields.Children.Count != 1;
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