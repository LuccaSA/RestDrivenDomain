using NExtends.Primitives;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;

namespace RDD.Web.Contexts
{
	public class HttpRequestMessageWrapper : IRequestMessage
	{
		public HttpRequestMessage Request { get; private set; }

		public HttpRequestMessageWrapper(HttpRequestMessage request)
		{
			Request = request;
		}

		public string Content { get { return Request.Content.ReadAsStringAsync().Result; } }
		public string ContentType { get { return Request.Headers.Accept.FirstOrDefault().MediaType; } }
		public Dictionary<string, string> ContentAsFormDictionnary { get { return Request.Content.ReadAsFormDataAsync().Result.ToDictionary(); } }

		public HttpResponseMessage CreateResponse<TEntity>(HttpStatusCode status, TEntity entity, MediaTypeFormatter formatter)
		{
			return Request.CreateResponse(status, entity, formatter);
		}
		public HttpResponseMessage CreateResponse(HttpStatusCode status, string message)
		{
			return Request.CreateResponse(status, message);
		}
	}
}
