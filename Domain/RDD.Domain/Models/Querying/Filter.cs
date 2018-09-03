using System;
using System.Linq.Expressions;

namespace RDD.Domain.Models.Querying
{
    public class Filter<TEntity>
    {
        public Expression<Func<TEntity, bool>> Expression { get; protected set; }

        public Filter() : this(e => true) { }
        public Filter(Expression<Func<TEntity, bool>> expression)
        {
            Expression = expression;
        }

        public static implicit operator Filter<TEntity>(Expression<Func<TEntity, bool>> expression) => new Filter<TEntity>(expression);
        public static implicit operator Expression<Func<TEntity, bool>>(Filter<TEntity> filter) => filter.Expression;
    }
}