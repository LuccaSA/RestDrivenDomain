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
        public LambdaExpression LambdaExpression { get; }
        public SortDirection Direction { get; }

        public static OrderBy<T> Ascending<TProp>(Expression<Func<T, TProp>> expression)
            => new OrderBy<T>(expression, SortDirection.Ascending);

        public static OrderBy<T> Descending<TProp>(Expression<Func<T, TProp>> expression)
            => new OrderBy<T>(expression, SortDirection.Descending);

        public OrderBy(LambdaExpression lambdaExpression, SortDirection direction = SortDirection.Ascending)
        {
            LambdaExpression = lambdaExpression;
            Direction = direction;
        }

        public IOrderedQueryable<T> ApplyOrderBy(IQueryable<T> source)
        {
            //https://www.tabsoverspaces.com/229310-sorting-in-iqueryable-using-string-as-column-name?utm_source=blog.cincura.net
            //https://stackoverflow.com/questions/3138133/what-is-the-purpose-of-linqs-expression-quote-method

            var anotherLevel = typeof(IOrderedQueryable).IsAssignableFrom(source.Expression.Type)
                && !typeof(EnumerableQuery).IsAssignableFrom(source.Expression.Type);
            var methodName = (!anotherLevel ? "OrderBy" : "ThenBy") + (Direction == SortDirection.Descending ? "Descending" : string.Empty);
            var correctedExpression = CorrectExpression();
            var call = Expression.Call(typeof(Queryable), methodName, new[] { typeof(T), correctedExpression.ReturnType }, source.Expression, Expression.Quote(correctedExpression));

            return (IOrderedQueryable<T>)source.Provider.CreateQuery<T>(call);
        }

        protected virtual LambdaExpression CorrectExpression()
        {
            if (LambdaExpression.ReturnType != typeof(Guid) && LambdaExpression.ReturnType != typeof(Guid?))
            {
                return LambdaExpression;
            }

            //https://github.com/aspnet/EntityFrameworkCore/issues/10198
            var method = LambdaExpression.ReturnType.GetMethod(nameof(Guid.ToString), new Type[] { });
            var call = Expression.Call(LambdaExpression.Body, method);
            return Expression.Lambda(call, LambdaExpression.Parameters[0]);
        }
    }
}