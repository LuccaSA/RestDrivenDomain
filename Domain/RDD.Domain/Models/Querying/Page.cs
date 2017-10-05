using RDD.Domain.Exceptions;
using System.Net;

namespace RDD.Domain.Models.Querying
{
	public class Page
	{
		public const int MAX_LIMIT = 1000;
		public static Page DEFAULT { get { return new Page(0, 10); } }

		public int Offset { get; private set; }
		public int Limit { get; private set; }
		public int TotalCount { get; set; }

		public Page(int offset, int limit)
			: this(offset, limit, MAX_LIMIT) { }

		protected Page(int offset, int limit, int maxLimit)
		{
			var offsetConnditions = offset >= 0;
			if (!offsetConnditions)
			{
				throw new HttpLikeException(HttpStatusCode.BadRequest,
					"Paging offset should be greater than 0");
			}

			var limitConditions = limit >= 1 && limit <= maxLimit;
			if (!limitConditions)
			{
				throw new HttpLikeException(HttpStatusCode.BadRequest,
					$"Paging limit should be between 1 and {maxLimit}");
			}

			Offset = offset;
			Limit = limit;
		}
	}
}
