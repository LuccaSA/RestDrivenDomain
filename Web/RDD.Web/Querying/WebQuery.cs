using RDD.Domain;
using RDD.Domain.Models.Querying;
using RDD.Web.Helpers;
using System;

namespace RDD.Web.Querying
{
    public class WebQuery<TEntity, TKey> : Query<TEntity>
        where TEntity : class, IEntityBase<TEntity, TKey>
        where TKey : IEquatable<TKey>
    {
        public WebQuery() : base() { }
        public WebQuery(params Filter<TEntity>[] filters)
            : base()
        {
            Filters = new PredicateService<TEntity, TKey>(filters).GetEntityPredicate(new QueryBuilder<TEntity, TKey>());
        }
        public WebQuery(Query<TEntity> source)
            : base(source) { }
    }
}
