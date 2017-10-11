using RDD.Domain;
using RDD.Domain.Models.Querying;
using RDD.Infra;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public Query<TEntity> FromWebContext(IWebContext webContext, bool isCollectionCall)
        {
            var parameters = webContext.GetQueryNameValuePairs().Where(v => !IgnoredFilters.Contains(v.Key)).ToDictionary(k => k.Key.ToLower(), k => k.Value);

            var fields = new FieldsParser().ParseFields<TEntity>(parameters, isCollectionCall);
            var collectionFields = new CollectionFieldsParser().ParseFields<ISelection<TEntity>>(parameters, isCollectionCall);
            var filters = new FiltersParser<TEntity>().Parse(parameters);
            var orderBys = new OrderByParser<TEntity>().Parse(parameters);
            var options = new OptionsParser<TEntity>().Parse(parameters, fields, collectionFields);
            var page = new PageParser<TEntity>().Parse(parameters);
            var headers = new HeadersParser().Parse(webContext.Headers);

            return new Query<TEntity>()
            {
                Fields = fields,
                Filters = filters,
                OrderBys = orderBys,
                Options = options,
                Page = page,
                Headers = headers,
            };
        }
    }
}
