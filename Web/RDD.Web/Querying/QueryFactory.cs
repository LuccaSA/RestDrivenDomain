using RDD.Domain;
using RDD.Domain.Models.Querying;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace RDD.Web.Querying
{
    public class QueryFactory<TEntity>
        where TEntity : class, IEntityBase
    {
        protected HashSet<string> IgnoredFilters { get; set; }

        public QueryFactory()
        {
            IgnoredFilters = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        public void IgnoreFilters(params string[] filters)
        {
            foreach (var filter in filters)
            {
                IgnoredFilters.Add(filter);
            }
        }

        public WebQuery<TEntity> FromWebContext(HttpContext httpContext, bool isCollectionCall)
        {
            var parameters = httpContext.GetQueryNameValuePairs().Where(v => !IgnoredFilters.Contains(v.Key)).ToDictionary(k => k.Key.ToLower(), k => k.Value);

            var fields = new FieldsParser().ParseFields<TEntity>(parameters, isCollectionCall);
            var collectionFields = new CollectionFieldsParser().ParseFields<ISelection<TEntity>>(parameters, isCollectionCall);
            var filters = new FiltersParser<TEntity>().Parse(parameters);
            var orderBys = new OrderByParser<TEntity>().Parse(parameters);
            var options = new OptionsParser<TEntity>().Parse(parameters, fields, collectionFields);
            var page = new PageParser<TEntity>().Parse(parameters);
            var headers = new HeadersParser().Parse(httpContext.Request.Headers);

            return new WebQuery<TEntity>()
            {
                Fields = fields,
                Filters = new FiltersConvertor<TEntity>().Convert(filters),
                OrderBys = orderBys,
                Options = options,
                Page = page,
                Headers = headers,
            };
        }
    }
}
