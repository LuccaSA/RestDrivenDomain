using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json.Serialization;
using RDD.Domain;
using RDD.Domain.Contexts;
using RDD.Domain.Exceptions;
using RDD.Web.Serialization;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Web.Exceptions
{
    public class JsonExceptionAttribute : ExceptionFilterAttribute
	{
		public override Task OnExceptionAsync(ExceptionContext context)
		{
			var baseException = context.Exception.GetBaseException();

			var httpException = HttpLikeException.Parse(baseException);

			var resolver = Resolver.Current();

			var formatter = JsonApiFormatter.GetInstance(resolver.Resolve<IWebContext>(), new CamelCasePropertyNamesContractResolver());

			var data = new EntitySerializer().SerializeExceptionWithStackTrace(httpException);

			var executionMode = resolver.Resolve<IExecutionModeProvider>().GetExecutionMode();

			if (executionMode == Domain.Helpers.ExecutionMode.ReleaseCandidate || executionMode == Domain.Helpers.ExecutionMode.Production)
			{
				data = new EntitySerializer().SerializeException(httpException);
			}

			context.HttpContext.Response.StatusCode = (int)httpException.Status;
			
			var stringContent = "";// await new ObjectContent<object>(data, formatter).ReadAsStringAsync();
			var content = Encoding.UTF8.GetBytes(stringContent);

			return context.HttpContext.Response.Body.WriteAsync(content, 0, content.Length);
		}
	}
}
