using Rdd.Domain.Helpers;
using Rdd.Domain.Helpers.Expressions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Rdd.Domain.Models.Querying
{
    public class Query<TEntity>
        where TEntity : class
    {
        public HttpVerbs Verb { get; set; }
        public IExpressionTree<TEntity> Fields { get; set; }
        public Filter<TEntity> Filter { get; set; }
        public List<OrderBy<TEntity>> OrderBys { get; set; }
        public Page Page { get; set; }
        public Options Options { get; set; }
        
        /// <summary>
        /// Warning : use only in case of multiple includes, and by testing the behavior before and after enabling this.
        /// This property can lead to under-perform in some cases, so use it with caution
        /// https://entityframework-plus.net/query-include-optimized
        /// </summary>
        public bool OptimizeIncludes { get; set; }

        public Query()
        {
            Verb = HttpVerbs.Get;
            Fields = new ExpressionTree<TEntity>();
            Filter = new Filter<TEntity>();
            OrderBys = new List<OrderBy<TEntity>>();
            Options = new Options();
            Page = Page.Unlimited;
        }

        public Query(IExpressionTree<TEntity> fields, List<OrderBy<TEntity>> orderBys, Page page, Filter<TEntity> filters, HttpVerbs httpVerbs)
        {
            Fields = fields;
            OrderBys = orderBys;
            Page = page;
            Filter = filters;
            Verb = httpVerbs;
            Options = new Options();
        }

        public Query(Expression<Func<TEntity, bool>> filter)
            : this()
        {
            Filter = filter;
        }

        public Query(Query<TEntity> source)
            : this()
        {
            Verb = source.Verb;
            Fields = source.Fields;
            Filter = source.Filter;
            OrderBys = source.OrderBys;
            Page = source.Page;
            Options = source.Options;
        }

        public Query(Query<TEntity> source, Expression<Func<TEntity, bool>> filter)
            : this(source)
        {
            Filter = new Filter<TEntity>(filter);
        }
    }
}