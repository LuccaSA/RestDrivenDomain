using RDD.Domain.Helpers;
using System;
using System.Linq.Expressions;

namespace RDD.Domain.Models.Querying
{
    public class Filter<TEntity>
    {
        public Expression<Func<TEntity, bool>> Expression { get; protected set; }

        public Filter()
        {
            Expression = e => true;
        }
        public Filter(Expression<Func<TEntity, bool>> expression)
        {
            Expression = expression;
        }
        public static implicit operator Filter<TEntity>(Expression<Func<TEntity, bool>> expression)
        {
            return new Filter<TEntity>(expression);
        }

        public virtual bool HasFilter(PropertySelector<TEntity> filter)
        {
            return false;
        }
    }
}