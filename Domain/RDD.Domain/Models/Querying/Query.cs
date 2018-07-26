using RDD.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RDD.Domain.Models.Querying
{
    public class Query<TEntity>
        where TEntity : class
    {
        public HttpVerbs Verb { get; set; }
        public Filter<TEntity> Filter { get; set; }
        public Queue<OrderBy<TEntity>> OrderBys { get; set; } 
        public Headers Headers { get; set; } 

        public Query()
        { 
            Verb = HttpVerbs.Get;
            Filter = new Filter<TEntity>();
            OrderBys = new Queue<OrderBy<TEntity>>(); 
        }

        public Query(Expression<Func<TEntity, bool>> filter)
            : this()
        {
            Filter = new Filter<TEntity>(filter);
            Page = Page.Max;
        }

        public Query(Query<TEntity> source)
            : this()
        {
            Verb = source.Verb;
            Filter = source.Filter;
            OrderBys = source.OrderBys; 
        }

        public Query(Query<TEntity> source, Expression<Func<TEntity, bool>> filter)
            : this(source)
        {
            Page = Page.Max;
            Filter = new Filter<TEntity>(filter);
        }
    }
}
