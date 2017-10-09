using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;

namespace RDD.Domain.Models.Querying
{
	public class Headers
	{
		public Headers()
		{
			RawHeaders = new Dictionary<string, StringValues>();
		}

		public DateTime? IfUnmodifiedSince { get; set; }
		public string Authorization { get; set; }
		public string ContentType { get; set; }

		public IEnumerable<KeyValuePair<string, StringValues>> RawHeaders { get; set; }
	}
}