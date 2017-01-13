using System.Collections.Specialized;

namespace RDD.Domain.Models.Querying
{
	public class Headers
	{
		public Headers()
		{
			RawHeaders = new NameValueCollection();
		}

		public string IfUnmodifiedSince { get; set; }
		public string Authorization { get; set; }
		public string ContentType { get; set; }

		public NameValueCollection RawHeaders { get; set; }

		public static Headers Parse(NameValueCollection requestHeaders)
		{
			var headers = new Headers();

			foreach (var key in requestHeaders.AllKeys)
			{
				switch (key)
				{
					case "If-Unmodified-Since":
						headers.IfUnmodifiedSince = requestHeaders[key];
						break;

					case "Authorization":
						headers.Authorization = requestHeaders[key];
						break;

					case "Content-Type":
						headers.ContentType = requestHeaders[key];
						break;
				}

				headers.RawHeaders.Add(key, requestHeaders[key]);
			}

			return headers;
		}
	}
}
