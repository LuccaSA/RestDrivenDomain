using RDD.Domain;
using RDD.Domain.Exceptions;
using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying;
using System.Collections.Generic;
using RDD.Web.Middleware;

namespace RDD.Web.Querying
{
    public class OrderByParser<TEntity>
        where TEntity : class
    {
        public Queue<OrderBy<TEntity>> Parse(Dictionary<string, string> parameters)
        {
            if (parameters.ContainsKey(QueryTokens.OrderBy))
            {
                return Parse(parameters[QueryTokens.OrderBy]);
            }
            return new Queue<OrderBy<TEntity>>();
        }

        protected Queue<OrderBy<TEntity>> Parse(string value)
        {
            var orders = value.Split(',');
            var length = orders.Length;
            var queue = new Queue<OrderBy<TEntity>>();

            //Il faut forcément un nb pair d'orders
            if (length % 2 == 0)
            {
                for (var i = 0; i < length; i += 2)
                {
                    var orderProperty = new PropertySelector<TEntity>();
                    orderProperty.Parse(orders[i].ToLower());

                    var orderDirection = orders[i + 1].ToLower();

                    if (orderDirection == "asc" || orderDirection == "desc")
                    {
                        queue.Enqueue(new OrderBy<TEntity>(orderProperty, (orderDirection == "desc" ? SortDirection.Descending : SortDirection.Ascending)));
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
