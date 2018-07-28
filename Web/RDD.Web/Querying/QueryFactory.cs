using RDD.Domain;
using RDD.Domain.Models.Querying;
using RDD.Infra.Web.Models;
using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace RDD.Web.Querying
{
    /// <summary>
    /// Factory to generate Query<T/>.
    /// </summary>
    public class QueryFactory : IQueryFactory
    {
        private readonly QueryParsers _queryParsers;
        private readonly QueryMetadata _queryMetadata;
        private readonly IOptions<RddOptions> _rddOptions;
       
        public QueryFactory(QueryMetadata queryMetadata, QueryParsers queryParsers, IOptions<RddOptions> rddOptions)
        {
            _queryMetadata = queryMetadata;
            _rddOptions = rddOptions;
            _queryParsers = queryParsers;
        }

        public Query<TEntity> NewFromHttpRequest<TEntity, TKey>()
            where TEntity : class, IPrimaryKey<TKey>
        {
            return new Query<TEntity>(
                new WebFiltersContainer<TEntity, TKey>(_queryParsers.WebFilterParser.ParseWebFilters<TEntity>()),
                new Queue<OrderBy<TEntity>>(_queryParsers.OrderByParser.ParseOrderBys<TEntity>()),
                _queryParsers.HeaderParser.ParseHeaders(),
                _queryParsers.PagingParser.ParsePaging(),
                _queryMetadata
            );
        }

        public Query<TEntity> New<TEntity>()
            where TEntity : class
        {
            return new Query<TEntity>(new QueryPaging(_rddOptions.Value));
        }
    }
}
