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

        public virtual bool HasFilter(Expression<Func<TEntity, object>> property)
        {
            throw new NotImplementedException("Expression filters cannot say if they contains individual filters.");
        }

        public virtual void RemoveFilter(Expression<Func<TEntity, object>> property)
        {
            throw new NotImplementedException("Expression filters cannot remove individual filters.");
        }
    }
}