using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Web
{
	public interface IRequestMessage
	{
		string Content { get; }
		string ContentType { get; }
		Dictionary<string, string> ContentAsFormDictionnary { get; }
		HttpResponseMessage CreateResponse<TEntity>(HttpStatusCode status, TEntity entity, MediaTypeFormatter formatter);
		HttpResponseMessage CreateResponse(HttpStatusCode status, string message);
	}
}
