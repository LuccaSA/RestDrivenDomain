using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using RDD.Domain.Exceptions;
using RDD.Domain.Helpers;
using RDD.Domain.Helpers.Expressions;
using RDD.Domain.Models.Querying;

namespace RDD.Web.Querying
{
    public class OrderByParser : IOrderByParser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OrderByParser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public IEnumerable<OrderBy<TEntity>> ParseOrderBys<TEntity>()
            where TEntity : class
        {
            if (!_httpContextAccessor.HttpContext.Request.Query.TryGetValue(QueryTokens.OrderBy, out StringValues orderByValues))
            {
                yield break;
            }
            var parser = new ExpressionParser();
            foreach (var clause in orderByValues)
            {
                var orders = clause.Split(',');
                var length = orders.Length;

                if (length % 2 != 0)
                {
                    throw new BadRequestException("Orders must contains order direction (asc or desc) for each field");
                }

                for (var i = 0; i < length; i += 2)
                {
                    var orderProperty = parser.Parse<TEntity>(orders[i].ToLower());

                    SortDirection direction;

                    if (string.Equals(orders[i + 1], "asc", StringComparison.InvariantCultureIgnoreCase))
                    {
                        direction = SortDirection.Ascending;
                    }
                    else if (string.Equals(orders[i + 1], "desc", StringComparison.InvariantCultureIgnoreCase))
                    {
                        direction = SortDirection.Ascending;
                    }
                    else
                    {
                        throw new BadRequestException("Order direction must match asc or desc");
                    }

                    yield return new OrderBy<TEntity>(orderProperty, direction);
                }
            }
        }
    }
}