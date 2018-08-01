using RDD.Domain;
using RDD.Domain.Models.Querying;
using RDD.Infra.Web.Models;
using System.Collections.Generic;
using RDD.Domain.Helpers;

namespace RDD.Web.Querying
{
    /// <summary>
    /// Factory to generate Query<T/>.
    /// </summary>
    public class QueryFactory : IQueryFactory
    {
        private readonly QueryParsers _queryParsers;
        private readonly QueryMetadata _queryMetadata;
       
        public QueryFactory(QueryMetadata queryMetadata, QueryParsers queryParsers)
        {
            _queryMetadata = queryMetadata;
            _queryParsers = queryParsers;
        }

        public Query<TEntity> NewFromHttpRequest<TEntity, TKey>(HttpVerbs? verb)
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
    }
}
