using Microsoft.Extensions.Primitives;
using RDD.Domain.Models.Querying;
using System;
using System.Collections.Generic;

namespace RDD.Web.QueryParsers
{
	public class HeadersParser
	{
		public Headers Parse(IEnumerable<KeyValuePair<string, StringValues>> requestHeaders)
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
