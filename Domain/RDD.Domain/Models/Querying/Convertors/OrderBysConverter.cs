using RDD.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace RDD.Domain.Models.Querying.Convertors
{
    public class OrderBysConverter<TEntity>
        where TEntity : class, IEntityBase
    {
        public IQueryable<TEntity> Convert(IQueryable<TEntity> entities, Queue<OrderBy<TEntity>> orderBys)
        {
            var safeCopy = new Queue<OrderBy<TEntity>>(orderBys);

            if (!safeCopy.Any())
            {
                throw new HttpLikeException(System.Net.HttpStatusCode.BadRequest, "There should be at least one orderBy instruction");
            }

            var first = safeCopy.Dequeue();

            return Convert(entities, first, safeCopy);
        }

        private IQueryable<TEntity> Convert(IQueryable<TEntity> entities, OrderBy<TEntity> first, Queue<OrderBy<TEntity>> orderBys)
        {
            var result = OrderBy(entities, first);

            foreach (var orderBy in orderBys)
            {
                result = OrderBy(result, orderBy, false);
            }

            return result;
        }

        protected virtual IQueryable<TEntity> OrderBy(IQueryable<TEntity> entities, OrderBy<TEntity> orderBy, bool isFirst = true)
        {
            var property = orderBy.Property.GetCurrentProperty();

            if (property.PropertyType == typeof(DateTime?))
            {
                return GetOrderyBy(entities, orderBy.Property.Lambda as Expression<Func<TEntity, DateTime?>>, orderBy.Direction, isFirst);
            }
            if (property.PropertyType == typeof(DateTime))
            {
                return GetOrderyBy(entities, orderBy.Property.Lambda as Expression<Func<TEntity, DateTime>>, orderBy.Direction, isFirst);
            }
            else if (property.PropertyType == typeof(int))
            {
                return GetOrderyBy(entities, orderBy.Property.Lambda as Expression<Func<TEntity, int>>, orderBy.Direction, isFirst);
            }
            else if (property.PropertyType.IsEnum)
            {
                return GetOrderyBy(entities, orderBy.Property.Lambda as Expression<Func<TEntity, int>>, orderBy.Direction, isFirst);
            }
            else
            {
                return GetOrderyBy(entities, orderBy.Property.Lambda as Expression<Func<TEntity, object>>, orderBy.Direction, isFirst);
            }
        }

        protected IQueryable<TEntity> GetOrderyBy<TPropertyType>(IQueryable<TEntity> entities, Expression<Func<TEntity, TPropertyType>> mySortExpression, SortDirection direction, bool isFirst)
        {
            if (isFirst)
            {
                return (direction == SortDirection.Descending) ? entities.OrderByDescending(mySortExpression)
                    : entities.OrderBy(mySortExpression);
            }
            else
            {
                return (direction == SortDirection.Descending) ? ((IOrderedQueryable<TEntity>)entities).ThenByDescending(mySortExpression)
                    : ((IOrderedQueryable<TEntity>)entities).ThenBy(mySortExpression);
            }
        }
    }
}
