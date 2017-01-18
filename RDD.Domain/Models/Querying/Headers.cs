using System;
using System.Collections.Specialized;

namespace RDD.Domain.Models.Querying
{
	public class Headers
	{
		public Headers()
		{
			RawHeaders = new NameValueCollection();
		}

		public DateTime? IfUnmodifiedSince { get; set; }
		public string Authorization { get; set; }
		public string ContentType { get; set; }

		public NameValueCollection RawHeaders { get; set; }

		public static Headers Parse(NameValueCollection requestHeaders)
		{
			var headers = new Headers();

			headers.RawHeaders = requestHeaders;

			foreach (var key in requestHeaders.AllKeys)
			{
				switch (key)
				{
					case "If-Unmodified-Since":
						DateTime unModifiedSince;
						if (DateTime.TryParse(requestHeaders[key], out unModifiedSince))
						{
							headers.IfUnmodifiedSince = unModifiedSince;
						}
						break;

					case "Authorization":
						headers.Authorization = requestHeaders[key];
						break;

					case "Content-Type":
						headers.ContentType = requestHeaders[key];
						break;
				}
			}

			return headers;
		}
	}
}
