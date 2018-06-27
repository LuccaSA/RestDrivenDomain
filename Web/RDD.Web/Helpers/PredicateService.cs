using LinqKit;
using RDD.Domain;
using RDD.Web.Querying;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace RDD.Web.Helpers
{
    internal class PredicateService<TEntity, TKey>
        where TEntity : class, IEntityBase<TEntity, TKey>
        where TKey : IEquatable<TKey>
    {
        private readonly ICollection<Filter<TEntity>> _filters;

        public PredicateService(ICollection<Filter<TEntity>> filters)
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

        private Expression<Func<TObject, bool>> ToExpression<TObject>(Filter<TEntity> filter, object value)
            where TObject : class
        {
            var filterProperty = filter.Property;
            var filterOperand = filter.Operand;

            switch (filterOperand)
            {
                case FilterOperand.Equals:

                    var type = typeof(TObject);
                    var property = type.GetProperties().Where(p => p.Name.ToLower() == filterProperty.Name.ToLower()).FirstOrDefault();

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

        private Expression<Func<TEntity, bool>> ToEntityExpression(QueryBuilder<TEntity, TKey> queryBuilder, Filter<TEntity> filter, IList value)
        {
            switch (filter.Operand)
            {
                case FilterOperand.Equals: return queryBuilder.Equals(filter.Property, value);
                case FilterOperand.NotEqual: return queryBuilder.NotEqual(filter.Property, value);
                case FilterOperand.Starts: return queryBuilder.Starts(filter.Property, value);
                case FilterOperand.Like: return queryBuilder.Like(filter.Property, value);
                case FilterOperand.Between: return queryBuilder.Between(filter.Property, value);
                case FilterOperand.Since: return queryBuilder.Since(filter.Property, value);
                case FilterOperand.Until: return queryBuilder.Until(filter.Property, value);
                case FilterOperand.Anniversary: return queryBuilder.Anniversary(filter.Property, value);
                case FilterOperand.GreaterThan: return queryBuilder.GreaterThan(filter.Property, value);
                case FilterOperand.GreaterThanOrEqual: return queryBuilder.GreaterThanOrEqual(filter.Property, value);
                case FilterOperand.LessThan: return queryBuilder.LessThan(filter.Property, value);
                case FilterOperand.LessThanOrEqual: return queryBuilder.LessThanOrEqual(filter.Property, value);
                case FilterOperand.ContainsAll: return queryBuilder.ContainsAll(filter.Property, value);
                default:
                    throw new NotImplementedException($"Unhandled operand : {filter.Operand}");
            }
        }
    }
}
