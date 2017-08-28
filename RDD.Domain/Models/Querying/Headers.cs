using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

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

		public static Headers Parse(IEnumerable<KeyValuePair<string, StringValues>> requestHeaders)
		{
			var headers = new Headers();

			headers.RawHeaders = requestHeaders;

			foreach (var element in requestHeaders)
			{
				switch (element.Key)
				{
					case "If-Unmodified-Since":
						DateTime unModifiedSince;
						if (DateTime.TryParse(element.Value, out unModifiedSince))
						{
							headers.IfUnmodifiedSince = unModifiedSince;
						}
						break;

					case "Authorization":
						headers.Authorization = element.Value;
						break;

					case "Content-Type":
						headers.ContentType = element.Value;
						break;
				}
			}

			return headers;
		}
	}
}
