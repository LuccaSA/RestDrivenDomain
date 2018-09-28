﻿using RDD.Domain.Helpers;
using RDD.Domain.Helpers.Expressions;
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
        public HttpVerb Verb { get; set; }
        public IExpressionTree<TEntity> Fields { get; set; }
        public Filter<TEntity> Filter { get; set; }
        public List<OrderBy<TEntity>> OrderBys { get; set; }
        public Page Page { get; set; }
        public Headers Headers { get; set; }
        public Options Options { get; set; }

        public Query()
        {
            Watch = new Stopwatch();
            Verb = HttpVerb.Get;
            Fields = new ExpressionTree<TEntity>();
            Filter = new Filter<TEntity>();
            OrderBys = new List<OrderBy<TEntity>>();
            Options = new Options();
            Page = Page.Unlimited;
        }

        public Query(Expression<Func<TEntity, bool>> filter)
            : this()
        {
            Filter = new Filter<TEntity>(filter);
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
