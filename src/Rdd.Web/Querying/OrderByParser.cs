using Rdd.Domain.Exceptions;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Domain.Models.Querying;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Rdd.Infra.Storage;

namespace Rdd.Web.Querying
{
    public class OrderByParser<TEntity> : IOrderByParser<TEntity>
         where TEntity : class
    {
        protected static readonly IReadOnlyDictionary<string, SortDirection> DirectionsByKeyword = new Dictionary<string, SortDirection>(StringComparer.OrdinalIgnoreCase)
        {
            { "asc", SortDirection.Ascending },
            { "desc", SortDirection.Descending }
        };

        protected IExpressionParser ExpressionParser { get; private set; }
        protected IPropertyAuthorizer<TEntity> PropertyAuthorizer { get; private set; }

        public OrderByParser(IExpressionParser expressionParser, IPropertyAuthorizer<TEntity> propertyAuthorizer)
        {
            ExpressionParser = expressionParser ?? throw new ArgumentNullException(nameof(expressionParser));
            PropertyAuthorizer = propertyAuthorizer ?? throw new ArgumentNullException(nameof(propertyAuthorizer));
        }

        public virtual List<OrderBy<TEntity>> Parse(HttpRequest request)
        {
            if (!request.Query.TryGetValue(Reserved.Orderby, out var value) || StringValues.IsNullOrEmpty(value))
            {
                return new List<OrderBy<TEntity>>();
            }

            var orders = value.ToString().Split(',');
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

                var expression = ExpressionParser.ParseChain<TEntity>(orders[i]);
                if (!PropertyAuthorizer.IsVisible(expression))
                {
                    throw new BadRequestException($"OrderBy parsing failed for {orders[i]}.", new ForbiddenException("Selected property is forbidden."));
                }

                if (!expression.ResultType.IsValueType && expression.ResultType.GetInterface(nameof(IComparable)) == null)
                {
                    throw new BadRequestException("Order by query parameter is invalid", new FormatException("Selected property is not comparable and Order By cannot be applied."));
                }

                result.Add(new OrderBy<TEntity>(expression.ToLambdaExpression(), DirectionsByKeyword[orders[i + 1]]));
            }

            return result;
        }
    }
}