using Newtonsoft.Json.Serialization;
using RDD.Domain.Contexts;
using RDD.Domain.Exceptions;
using RDD.Web.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Filters;

namespace RDD.Web.Exceptions
{
	public class JsonExceptionAttribute : ExceptionFilterAttribute
	{
		public override void OnException(HttpActionExecutedContext actionExecutedContext)
		{
			var baseException = actionExecutedContext.Exception.GetBaseException();

			var httpException = HttpLikeException.Parse(baseException);

			var formatter = JsonApiFormatter.GetInstance(WebContext.Current(), new CamelCasePropertyNamesContractResolver());

			var data = new EntitySerializer().SerializeException(httpException);

			actionExecutedContext.Response = new HttpResponseMessage(httpException.Status) {  Content = new ObjectContent(typeof(HttpLikeException), httpException, formatter) };
		}
	}
}
