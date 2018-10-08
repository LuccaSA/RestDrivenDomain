using Rdd.Domain.Exceptions;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Domain.Models.Querying;
using System;
using System.Collections.Generic;

namespace Rdd.Web.Querying
{
    public class OrderByParser : IOrderByParser
    {
        protected static readonly IReadOnlyDictionary<string, SortDirection> DirectionsByKeyword = new Dictionary<string, SortDirection>(StringComparer.OrdinalIgnoreCase)
        {
            { "asc", SortDirection.Ascending },
            { "desc", SortDirection.Descending }
        };

        protected IExpressionParser ExpressionParser { get; private set; }

        public OrderByParser(IExpressionParser expressionParser)
        {
            ExpressionParser = expressionParser ?? throw new ArgumentNullException(nameof(expressionParser));
        }

        public virtual List<OrderBy<TEntity>> Parse<TEntity>(string value)
            where TEntity : class
        {
            var orders = (value ?? "").Split(',');
            if (orders.Length % 2 != 0)
            {
                throw new BadRequestException("Order by query parameter is invalid", new FormatException("Correct order by format is `orderby=(property,[asc|desc])*`"));
            }

            var result = new List<OrderBy<TEntity>>();
            for (var i = 0; i < orders.Length; i += 2)
            {
                if (!DirectionsByKeyword.ContainsKey(orders[i + 1]))
                {
                    throw new BadRequestException("Order by query parameter is invalid", new FormatException("Correct order by format is `orderby=(property,[asc|desc])*`"));
                }

                var expression = ExpressionParser.Parse<TEntity>(orders[i]);

                if (!expression.ResultType.IsValueType && expression.ResultType.GetInterface(nameof(IComparable)) == null)
                {
                    throw new BadRequestException("Order by query parameter is invalid", new FormatException("Selected property is not comparable and Order By cannot be applied."));
                }

                result.Add(new OrderBy<TEntity>(expression, DirectionsByKeyword[orders[i + 1]]));
            }

            return result;
        }
    }
}