using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying.Convertors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;

namespace RDD.Domain.Models.Querying
{
    public class Query<TEntity> : Query
        where TEntity : class, IEntityBase
    {
        public Query()
        {
            Fields = new Field<TEntity>();
            CollectionFields = new Field<ISelection<TEntity>>();
            Filters = new List<Filter<TEntity>>();
            OrderBys = new Queue<OrderBy<TEntity>>();
        }
     
        public List<Filter<TEntity>> Filters { get; set; }
        public Queue<OrderBy<TEntity>> OrderBys { get; set; }

        public Query(params Filter<TEntity>[] filters)
            : this()
        {
            Filters.AddRange(filters);
        }
        public Query(Query<TEntity> source)
            : this()
        {
            Filters = new List<Filter<TEntity>>(source.Filters);
            OrderBys = new Queue<OrderBy<TEntity>>(source.OrderBys);
        }

        public virtual Expression<Func<TEntity, bool>> FiltersAsExpression() => new FiltersConvertor<TEntity>().Convert(Filters);
    }

    public abstract class Query
    {
        protected Query()
        {
            Watch = new Stopwatch();
            Verb = HttpVerbs.Get;
            Options = new Options();
            Page = Page.Default;
        }

        public Stopwatch Watch { get; }
        public HttpVerbs Verb { get; set; }
        public Page Page { get; set; }
        public Headers Headers { get; set; }
        public Options Options { get; set; }

        public Field Fields { get; set; }
        public Field CollectionFields { get; set; }

        public int TotalCount { get; set; }
    }
}
