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
	}
}
