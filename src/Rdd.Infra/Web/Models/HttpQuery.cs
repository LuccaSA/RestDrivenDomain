using Rdd.Domain;
using Rdd.Domain.Helpers;
using Rdd.Domain.Helpers.Expressions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Rdd.Infra.Web.Models
{
    public class HttpQuery<TEntity, TKey> : Query<TEntity>
        where TEntity : class, IPrimaryKey<TKey>
        where TKey : IEquatable<TKey>
    {
        public HttpQuery()
            : base() { }

        public HttpQuery(Expression<Func<TEntity, bool>> filter)
            : base(filter) { }

        public HttpQuery(IExpressionTree<TEntity> fields, List<OrderBy<TEntity>> orderBys, Page page, Filter<TEntity> filters, HttpVerbs httpVerbs)
            : base(fields, orderBys, page, filters, httpVerbs) { }
    }
}