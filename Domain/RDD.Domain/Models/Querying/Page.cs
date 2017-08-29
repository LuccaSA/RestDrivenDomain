using RDD.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain.Models.Querying
{
	public class Page
	{
		public const int DEFAULT_LIMIT = 100;
		public static Page DEFAULT = new Page(0, Page.DEFAULT_LIMIT);

		[Range(0, int.MaxValue, ErrorMessage = "Offset should be positive")]
		public int Offset { get; private set; }
		[Range(1, int.MaxValue)]
		public int Limit { get; private set; }
		public int TotalCount { get; set; }

		public Page(int offset, int limit)
		{
			Offset = offset;
			Limit = limit;
		}

		public static Page Parse(string queryLimit)
		{
			if (queryLimit == "1") //...&paging=1 <=> &paging=0,100
			{
				return Page.DEFAULT;
			}
			else //...&paging=x,y
			{
				var elements = queryLimit.Split(',');

				if (elements.Length == 2)
				{
					int offset;
					int limit;

					if (!Int32.TryParse(elements[0], out offset))
					{
						throw new HttpLikeException(HttpStatusCode.BadRequest, String.Format("Offset value {0} not in correct format", elements[0]));
					}

					if (!Int32.TryParse(elements[1], out limit))
					{
						throw new HttpLikeException(HttpStatusCode.BadRequest, String.Format("Limit value {0} not in correct format", elements[1]));
					}

					return new Page(offset, limit);
				}
				else
				{
					throw new HttpLikeException(HttpStatusCode.BadRequest, String.Format("{0} does not respect limit=start,count format", queryLimit));
				}
			}
		}
	}
}
