using LinqKit;
using RDD.Domain;
using RDD.Infra.Web.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace RDD.Infra.Helpers
{
    public class PredicateService<TEntity, TKey>
        where TEntity : IPrimaryKey<TKey>
    {
        private readonly IEnumerable<WebFilter<TEntity>> _filters;

        public PredicateService(IEnumerable<WebFilter<TEntity>> filters)
        {
            _filters = filters;
        }

        public Expression<Func<TObject, bool>> GetPredicate<TObject>()
            where TObject : class
        {
            var feed = PredicateBuilder.True<TObject>();

            foreach (var filter in _filters)
            {
                var sub = PredicateBuilder.False<TObject>();

                foreach (var value in filter.Values)
                {
                    sub = sub.Or(ToExpression<TObject>(filter, value).Expand());
                }

                feed = feed.And(sub.Expand());
            }

            return feed.Expand();
        }

        private Expression<Func<TObject, bool>> ToExpression<TObject>(WebFilter<TEntity> filter, object value)
            where TObject : class
        {
            var filterProperty = filter.Selector;
            var filterOperand = filter.Operand;

            switch (filterOperand)
            {
                case WebFilterOperand.Equals:

                    var type = typeof(TObject);
                    var property = type
                        .GetProperties()
                        .FirstOrDefault(p => p.Name.ToLower() == filterProperty.Name.ToLower());

                    var parameter = Expression.Parameter(type, "entity");

                    var body = Expression.PropertyOrField(parameter, filterProperty.Name);

                    return Expression.Lambda<Func<TObject, bool>>(Expression.Equal(body, Expression.Constant(value, property.PropertyType)), parameter);

                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Fait des AND entre les where dont les différentes valeurs dont combinées via des OR
        /// /api/users?manager.id=2&departement.id=4,5 devient manager.id == 2 AND ( department.id == 4 OR department.id == 5 )
        /// </summary>
        /// <returns></returns>
        public Expression<Func<TEntity, bool>> GetEntityPredicate(QueryBuilder<TEntity, TKey> queryBuilder)
        {
            var feed = PredicateBuilder.True<TEntity>();
            foreach (var filter in _filters)
            {
                var expression = ToEntityExpression(queryBuilder, filter, filter.Values);
                if (expression != null)
                {
                    feed = feed.And(expression.Expand());
                }
            }

            return feed.Expand();
        }

        private Expression<Func<TEntity, bool>> ToEntityExpression(QueryBuilder<TEntity, TKey> queryBuilder, WebFilter<TEntity> filter, IList value)
        {
            switch (filter.Operand)
            {
                case WebFilterOperand.Equals: return queryBuilder.Equals(filter.Selector, value);
                case WebFilterOperand.NotEqual: return queryBuilder.NotEqual(filter.Selector, value);
                case WebFilterOperand.Starts: return queryBuilder.Starts(filter.Selector, value);
                case WebFilterOperand.Like: return queryBuilder.Like(filter.Selector, value);
                case WebFilterOperand.Between: return queryBuilder.Between(filter.Selector, value);
                case WebFilterOperand.Since: return queryBuilder.Since(filter.Selector, value);
                case WebFilterOperand.Until: return queryBuilder.Until(filter.Selector, value);
                case WebFilterOperand.Anniversary: return queryBuilder.Anniversary(filter.Selector, value);
                case WebFilterOperand.GreaterThan: return queryBuilder.GreaterThan(filter.Selector, value);
                case WebFilterOperand.GreaterThanOrEqual: return queryBuilder.GreaterThanOrEqual(filter.Selector, value);
                case WebFilterOperand.LessThan: return queryBuilder.LessThan(filter.Selector, value);
                case WebFilterOperand.LessThanOrEqual: return queryBuilder.LessThanOrEqual(filter.Selector, value);
                case WebFilterOperand.ContainsAll: return queryBuilder.ContainsAll(filter.Selector, value);
                default:
                    throw new NotImplementedException($"Unhandled operand : {filter.Operand}");
            }
        }
    }
}
