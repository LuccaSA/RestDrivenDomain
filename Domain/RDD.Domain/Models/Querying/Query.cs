using RDD.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RDD.Domain.Models.Querying
{
    public class Query
    {
        protected Query(HttpVerbs? verb, Headers headers = null, QueryPaging paging = null, QueryMetadata queryMetadata = null)
        {
            QueryMetadata = queryMetadata ?? new QueryMetadata();
            Verb = verb ?? HttpVerbs.None;
            Headers = headers ?? new Headers();
            Paging = paging ?? new QueryPaging(new RddOptions());

            // copy paging infos for metadatas
            QueryMetadata.Paging = paging == null ? null : new QueryMetadataPaging
            {
                ItemPerPage = Paging.ItemPerPage,
                PageOffset = Paging.PageOffset
            };

            NeedEnumeration = true;
            CheckRights = true;
            WithWarnings = true;
        }

        public HttpVerbs Verb { get; set; }
        public Headers Headers { get; }
        public QueryPaging Paging { get; }
        public QueryMetadata QueryMetadata { get; }

        /// <summary>
        /// Est-ce qu'on a besoin du Count
        /// </summary>
        public bool NeedCount { get; set; }

        /// <summary>
        /// Est-ce qu'on a besoin d'énumérer la query
        /// </summary>
        public bool NeedEnumeration { get; set; }

        /// <summary>
        /// Should we FilterRights on GET request, or CheckRightForCreate on POST
        /// </summary>
        public bool CheckRights { get; set; }

        public bool WithWarnings { get; set; }
    }

    public class Query<TEntity> : Query
        where TEntity : class
    {
        public Filter<TEntity> Filter { get; set; }
        public Queue<OrderBy<TEntity>> OrderBys { get; set; }

        public Query()
            : base(HttpVerbs.Get, new Headers())
        {
            Filter = new Filter<TEntity>();
            OrderBys = new Queue<OrderBy<TEntity>>();
        }
        
        public Query(QueryPaging paging)
            : base(HttpVerbs.Get, new Headers(), paging)
        {
            Filter = new Filter<TEntity>();
            OrderBys = new Queue<OrderBy<TEntity>>();
        }

        public Query(Filter<TEntity> filters, Queue<OrderBy<TEntity>> orderBys, Headers headers, QueryPaging paging, QueryMetadata queryMetadata)
            : base(HttpVerbs.Get, headers, paging, queryMetadata)
        {
            Filter = filters;
            OrderBys = orderBys;
        }

        public Query(Expression<Func<TEntity, bool>> filter, Headers headers = null, QueryPaging paging = null)
            : base(HttpVerbs.Get, headers, paging)
        {
            Filter = new Filter<TEntity>(filter);
            OrderBys = new Queue<OrderBy<TEntity>>();
        }

        public Query(Query<TEntity> source)
            : base(source.Verb, source.Headers, source.Paging)
        {
            Filter = source.Filter;
            OrderBys = source.OrderBys;
        }

        public Query(Query<TEntity> source, Expression<Func<TEntity, bool>> filter)
            : base(source.Verb, source.Headers, source.Paging)
        {
            Filter = new Filter<TEntity>(filter);
            OrderBys = source.OrderBys;
        }
    }
}
