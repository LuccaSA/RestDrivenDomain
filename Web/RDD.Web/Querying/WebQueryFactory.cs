using Rdd.Domain;
using Rdd.Infra.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rdd.Web.Querying
{
    public class WebQueryFactory<TEntity, TKey>
        where TEntity : class, IPrimaryKey<TKey>
    {
        protected HashSet<string> IgnoredFilters { get; set; }

        public WebQueryFactory()
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

        public WebQuery<TEntity> FromWebContext(IHttpContextHelper httpContextHelper, bool isCollectionCall)
        {
            var parameters = httpContextHelper.GetQueryNameValuePairs().Where(v => !IgnoredFilters.Contains(v.Key)).ToDictionary(k => k.Key.ToLower(), k => k.Value);

            var fields = new FieldsParser().ParseFields<TEntity>(parameters, isCollectionCall);
            var filters = WebFiltersParser<TEntity>.Parse(parameters);
            var orderBys = new OrderByParser<TEntity>().Parse(parameters);
            var options = new OptionsParser().Parse(parameters, fields);
            var page = new WebPageParser().Parse(parameters);
            var headers = new HeadersParser().Parse(httpContextHelper.GetHeaders());

            return new WebQuery<TEntity>
            {
                Fields = fields,
                Filter = new WebFiltersContainer<TEntity, TKey>(filters),
                OrderBys = orderBys,
                Options = options,
                Page = page,
                Headers = headers,
            };
        }
    }
}
