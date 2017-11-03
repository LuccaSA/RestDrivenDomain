using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying.Convertors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;

namespace RDD.Domain.Models.Querying
{
    public class Query<TEntity>
        where TEntity : class, IEntityBase
    {
        public Stopwatch Watch { get; }
        public HttpVerb Verb { get; set; }
        public Field<TEntity> Fields { get; set; }
        public Field<ISelection<TEntity>> CollectionFields { get; set; }
        public List<Filter<TEntity>> Filters { get; set; }
        public Queue<OrderBy<TEntity>> OrderBys { get; set; }
        public Page Page { get; set; }
        public Headers Headers { get; set; }
        public Options Options { get; set; }

        public Query()
        {
            Watch = new Stopwatch();
            Verb = HttpVerb.Get;
            Fields = new Field<TEntity>();
            CollectionFields = new Field<ISelection<TEntity>>();
            Filters = new List<Filter<TEntity>>();
            OrderBys = new Queue<OrderBy<TEntity>>();
            Options = new Options();
            Page = Page.Default;
        }
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
}
