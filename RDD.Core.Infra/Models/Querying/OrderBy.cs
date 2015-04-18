using RDD.Infra.Models.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Infra.Models.Querying
{
	public enum SortDirection { Ascending, Descending };

	public class OrderBy
	{
		public string Field { get; set; }
		public SortDirection Direction { get; set; }

		public static List<OrderBy> Parse(string queryStringValue)
		{
			var orders = queryStringValue.Split(',');
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
						list.Add(new OrderBy() { Field = orderField, Direction = (orderDirection == "desc" ? SortDirection.Descending : SortDirection.Ascending) });
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
