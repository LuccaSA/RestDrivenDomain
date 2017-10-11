using System;
using System.Linq.Expressions;

namespace RDD.Domain.Models.Querying
{
    public class ExpressionQuery<TEntity> : Query<TEntity>
        where TEntity : class, IEntityBase
    {
        public Expression<Func<TEntity, bool>> ExpressionFilters { get; set; }

        public ExpressionQuery(Expression<Func<TEntity, bool>> filters)
        {
            ExpressionFilters = filters;
        }
        public ExpressionQuery(Query<TEntity> source, Expression<Func<TEntity, bool>> filters)
            : this(filters)
        {
            Fields = source.Fields;
            OrderBys = source.OrderBys;
            Page = source.Page;
            Options = source.Options;
        }

        public override Expression<Func<TEntity, bool>> FiltersAsExpression() => ExpressionFilters;
    }
}
