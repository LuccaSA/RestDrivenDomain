using System;
using System.Linq;
using System.Linq.Expressions;

namespace RDD.Domain.Helpers
{
    public static class BooleanExpression
    {
        public static Expression<Func<TEntity, bool>> OrFactory<TEntity>(params Expression<Func<TEntity, bool>>[] filters)
        {
            return Factory((aggr, newExpr) => Expression.OrElse(aggr, newExpr), filters);
        }
        public static Expression<Func<TEntity, bool>> AndFactory<TEntity>(params Expression<Func<TEntity, bool>>[] filters)
        {
            return Factory((aggr, newExpr) => Expression.AndAlso(aggr, newExpr), filters);
        }

        private static Expression<Func<TEntity, bool>> Factory<TEntity>(Func<Expression, Expression, Expression> aggregator, params Expression<Func<TEntity, bool>>[] filters)
        {
            if (filters.Length == 0)
                return null;

            var seed = filters[0];
            if (filters.Length == 1)
                return seed;

            var param = seed.Parameters[0];
            var visitor = new ParameterChanger(param);
            var correctParamsFilters = filters.Select(f => visitor.Visit(f.Body));

            return Expression.Lambda<Func<TEntity, bool>>(correctParamsFilters.Skip(1).Aggregate(seed.Body, aggregator), param);
        }
    }
}
