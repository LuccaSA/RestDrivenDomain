using RDD.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;

namespace RDD.Domain.Models.Querying
{
    public class Query<TEntity>
        where TEntity : class
    {
        public Stopwatch Watch { get; }
        public HttpVerbs Verb { get; set; }
        public Field Fields { get; set; }
        public Field CollectionFields { get; set; }
        public Expression<Func<TEntity, bool>> Filters { get; set; }
        public Queue<OrderBy<TEntity>> OrderBys { get; set; }
        public Page Page { get; set; }
        public Headers Headers { get; set; }
        public Options Options { get; set; }

        public Query()
        {
            Watch = new Stopwatch();
            Verb = HttpVerbs.Get;
            Fields = new Field<TEntity>();
            Filters = e => true;
            CollectionFields = new Field<ISelection<TEntity>>();
            OrderBys = new Queue<OrderBy<TEntity>>();
            Options = new Options();
            Page = Page.Default;
        }
        public Query(Expression<Func<TEntity, bool>> filters)
            : this()
        {
            Filters = filters;
        }
        public Query(Query<TEntity> source)
            : this()
        {
            Verb = source.Verb;
            Fields = source.Fields;
            Filters = source.Filters;
            OrderBys = source.OrderBys;
            Page = source.Page;
            Options = source.Options;
        }
        public Query(Query<TEntity> source, Expression<Func<TEntity, bool>> filters)
            : this(source)
        {
            Filters = filters;
        }
    }
}
