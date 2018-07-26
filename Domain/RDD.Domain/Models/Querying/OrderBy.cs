using RDD.Domain.Helpers;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace RDD.Domain.Models.Querying
{
    public class OrderBy<TEntity>
        where TEntity : class
    {
        public PropertySelector Property { get; }
        public SortDirection Direction { get; }

        public OrderBy(Expression<Func<TEntity, object>> expression, SortDirection direction = SortDirection.Ascending)
            : this(new PropertySelector<TEntity>(expression), direction) { }

        public OrderBy(PropertySelector property, SortDirection direction = SortDirection.Ascending)
        {
            Property = property;
            Direction = direction;
        }
    }
}
