using RDD.Domain.Exceptions;
using RDD.Domain.Models.Querying;
using System.Collections.Generic;

namespace RDD.Web.QueryParsers
{
	class OrderByParser
	{
		public List<OrderBy> Parse(Dictionary<string, string> parameters)
		{
			if (parameters.ContainsKey(Reserved.orderby.ToString()))
			{
				return Parse(parameters[Reserved.orderby.ToString()]);
			}

			return new List<OrderBy>();
		}

		protected List<OrderBy> Parse(string value)
		{
			var orders = value.Split(',');
			var length = orders.Length;
			var list = new List<OrderBy>();

			//Il faut forcément un nb pair d'orders
			if (length % 2 == 0)
			{
				for (var i = 0; i < length; i += 2)
				{
					var orderField = orders[i].ToLower();
					var orderDirection = orders[i + 1].ToLower();

					if (orderDirection == "asc" || orderDirection == "desc")
					{
						list.Add(new OrderBy { Field = orderField, Direction = (orderDirection == "desc" ? SortDirection.Descending : SortDirection.Ascending) });
					}
					else
					{
						throw new HttpLikeException(System.Net.HttpStatusCode.BadRequest, "Order direction must match asc or desc");
					}
				}
			}
			else
			{
				throw new HttpLikeException(System.Net.HttpStatusCode.BadRequest, "Orders must contains order direction (asc or desc) for each field");
			}

			return list;
		}
	}
}
