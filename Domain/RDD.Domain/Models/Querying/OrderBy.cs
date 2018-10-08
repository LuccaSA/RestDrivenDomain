using Rdd.Domain.Helpers.Expressions;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Rdd.Domain.Models.Querying
{
    public enum SortDirection { Ascending, Descending };

    public class OrderBy<T>
        where T : class
    {
        public IExpression Expression { get; }
        public SortDirection Direction { get; }

        public static OrderBy<T> New<TProp>(Expression<Func<T, TProp>> expression, SortDirection direction = SortDirection.Ascending)
            => new OrderBy<T>(new PropertyExpression { LambdaExpression = expression }, direction);
        public static OrderBy<T> Ascending<TProp>(Expression<Func<T, TProp>> expression)
            => new OrderBy<T>(new PropertyExpression { LambdaExpression = expression }, SortDirection.Ascending);
        public static OrderBy<T> Descending<TProp>(Expression<Func<T, TProp>> expression)
            => new OrderBy<T>(new PropertyExpression { LambdaExpression = expression }, SortDirection.Descending);

        public OrderBy(IExpression expression, SortDirection direction = SortDirection.Ascending)
        {
            Expression = expression;
            Direction = direction;
        }

        public IOrderedQueryable<T> ApplyOrderBy(IQueryable<T> source)
        {
            //https://www.tabsoverspaces.com/229310-sorting-in-iqueryable-using-string-as-column-name?utm_source=blog.cincura.net
            //https://stackoverflow.com/questions/3138133/what-is-the-purpose-of-linqs-expression-quote-method

            var sort = Expression.ToLambdaExpression();
            var anotherLevel = typeof(IOrderedQueryable).IsAssignableFrom(source.Expression.Type)
                && !typeof(EnumerableQuery).IsAssignableFrom(source.Expression.Type);
            var methodName = (!anotherLevel ? "OrderBy" : "ThenBy") + (Direction == SortDirection.Descending ? "Descending" : string.Empty);
            var call = System.Linq.Expressions.Expression.Call(typeof(Queryable), methodName, new[] { typeof(T), Expression.ResultType }, source.Expression, System.Linq.Expressions.Expression.Quote(sort));

            return (IOrderedQueryable<T>)source.Provider.CreateQuery<T>(call);
        }
    }
}