﻿using RDD.Domain.Exceptions;
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
		public const int MAX_LIMIT = 1000;
		public static Page DEFAULT = new Page(0, 10);

		public int Offset { get; private set; }
		public int Limit { get; private set; }
		public int TotalCount { get; set; }

		public Page(int offset, int limit)
			: this(offset, limit, MAX_LIMIT) { }

		protected Page(int offset, int limit, int maxLimit)
		{
			var offsetConnditions = offset >= 0 && offset < maxLimit;
			if (!offsetConnditions)
			{
				throw new HttpLikeException(HttpStatusCode.BadRequest,
					$"Paging offset should be between 0 and {maxLimit - 1}");
			}

			var limitConditions = limit >= 1 && limit <= maxLimit;
			if (!limitConditions)
			{
				throw new HttpLikeException(HttpStatusCode.BadRequest,
					$"Paging limit should be between 1 and {maxLimit - 1}");
			}

			Offset = offset;
			Limit = limit;
		}
	}
}