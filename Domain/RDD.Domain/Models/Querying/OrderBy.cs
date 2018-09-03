using RDD.Domain.Helpers.Expressions;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace RDD.Domain.Models.Querying
{
    public enum SortDirection { Ascending, Descending };

    public class OrderBy<T>
        where T : class
    {
        public IExpressionSelector Selector { get; }
        public SortDirection Direction { get; }

        public static OrderBy<T> New<TProp>(Expression<Func<T, TProp>> selector, SortDirection direction = SortDirection.Ascending)
            => new OrderBy<T>(new SimplePropertySelector { LambdaExpression = selector }, direction);
        public static OrderBy<T> Ascending<TProp>(Expression<Func<T, TProp>> selector)
            => new OrderBy<T>(new SimplePropertySelector { LambdaExpression = selector }, SortDirection.Ascending);
        public static OrderBy<T> Descending<TProp>(Expression<Func<T, TProp>> selector)
            => new OrderBy<T>(new SimplePropertySelector { LambdaExpression = selector }, SortDirection.Descending);

        public OrderBy(IExpressionSelector selector, SortDirection direction = SortDirection.Ascending)
        {
            Selector = selector;
            Direction = direction;
        }

        public IOrderedQueryable<T> ApplyOrderBy(IQueryable<T> source)
        {
            //https://www.tabsoverspaces.com/229310-sorting-in-iqueryable-using-string-as-column-name?utm_source=blog.cincura.net
            //https://stackoverflow.com/questions/3138133/what-is-the-purpose-of-linqs-expression-quote-method

            var sort = Selector.ToLambdaExpression();
            var anotherLevel = typeof(IOrderedQueryable<>).IsAssignableFrom(source.GetType().GetGenericTypeDefinition());
            var methodName = (!anotherLevel ? "OrderBy" : "ThenBy") + (Direction == SortDirection.Descending ? "Descending" : string.Empty);
            var call = Expression.Call(typeof(Queryable), methodName, new[] { typeof(T), Selector.ResultType }, source.Expression, Expression.Quote(sort));

            return (IOrderedQueryable<T>)source.Provider.CreateQuery<T>(call);
        }
    }
}
