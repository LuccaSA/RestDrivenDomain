using Rdd.Domain.Exceptions;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Domain.Models.Querying;
using System.Collections.Generic;

namespace Rdd.Web.Querying
{
    public class OrderByParser<TEntity>
        where TEntity : class
    {
        public List<OrderBy<TEntity>> Parse(Dictionary<string, string> parameters)
        {
            if (parameters.ContainsKey(Reserved.orderby.ToString()))
            {
                return Parse(parameters[Reserved.orderby.ToString()]);
            }

            return new List<OrderBy<TEntity>>();
        }

        protected List<OrderBy<TEntity>> Parse(string value)
        {
            var parser = new ExpressionParser();
            var orders = value.Split(',');
            var length = orders.Length;
            var queue = new List<OrderBy<TEntity>>();

            //Il faut forcément un nb pair d'orders
            if (length % 2 == 0)
            {
                for (var i = 0; i < length; i += 2)
                {
                    var orderProperty = parser.Parse<TEntity>(orders[i].ToLower());
                    var orderDirection = orders[i + 1].ToLower();

                    if (orderDirection == "asc" || orderDirection == "desc")
                    {
                        queue.Add(new OrderBy<TEntity>(orderProperty.ToLambdaExpression(), orderDirection == "desc" ? SortDirection.Descending : SortDirection.Ascending));
                    }
                    else
                    {
                        throw new BadRequestException("Order direction must match asc or desc");
                    }
                }
            }
            else
            {
                throw new BadRequestException("Orders must contains order direction (asc or desc) for each field");
            }

            return queue;
        }
    }
}
