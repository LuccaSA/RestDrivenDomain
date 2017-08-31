using RDD.Domain.Exceptions;
using System.Net;
using Xunit;

namespace RDD.Domain.Tests
{
	public class HttpLikeExceptionsTests
	{
		public HttpLikeExceptionsTests()
		{
		}

		[Fact]
		public void Exception_SHOULD_work_WHEN_WithStatusAndMessage()
		{
			var exception = new HttpLikeException(HttpStatusCode.Conflict, "My message");
		}

		[Fact]
		public void Exception_SHOULD_work_WHEN_WithStatusAndComplexMessage()
		{
			var message = @"{""Status"":400,""Message"":""authToken invalide"",""StackTrace"":""   at iLucca.Areas.auth.Controllers.impersonationController.Get(String authToken, String userKey, String userKeyType, String Continue) in c:\\d\\sites\\ilucca.net\\iLucca\\Areas\\auth\\Controllers\\impersonationController.cs:line 68\r\n   at iLucca.Areas.auth.Controllers.impersonationController.Get(String authToken, String userKey, String userKeyType) in c:\\d\\sites\\ilucca.net\\iLucca\\Areas\\auth\\Controllers\\impersonationController.cs:line 36\r\n   at lambda_method(Closure , Object , Object[] )\r\n   at System.Web.Http.Controllers.ReflectedHttpActionDescriptor.ActionExecutor.<>c__DisplayClass10.<GetExecutor>b__9(Object instance, Object[] methodParameters)\r\n   at System.Web.Http.Controllers.ReflectedHttpActionDescriptor.ActionExecutor.Execute(Object instance, Object[] arguments)\r\n   at System.Web.Http.Controllers.ReflectedHttpActionDescriptor.ExecuteAsync(HttpControllerContext controllerContext, IDictionary`2 arguments, CancellationToken cancellationToken)\r\n--- End of stack trace from previous location where exception was thrown ---\r\n   at System.Runtime.CompilerServices.TaskAwaiter.ThrowForNonSuccess(Task task)\r\n   at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)\r\n   at System.Web.Http.Controllers.ApiControllerActionInvoker.<InvokeActionAsyncCore>d__0.MoveNext()\r\n--- End of stack trace from previous location where exception was thrown ---\r\n   at System.Runtime.CompilerServices.TaskAwaiter.ThrowForNonSuccess(Task task)\r\n   at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)\r\n   at System.Web.Http.Controllers.ActionFilterResult.<ExecuteAsync>d__2.MoveNext()\r\n--- End of stack trace from previous location where exception was thrown ---\r\n   at System.Runtime.CompilerServices.TaskAwaiter.ThrowForNonSuccess(Task task)\r\n   at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)\r\n   at System.Web.Http.Controllers.ExceptionFilterResult.<ExecuteAsync>d__0.MoveNext()""}";
			var exception = new HttpLikeException(HttpStatusCode.Conflict, message);
		}
	}
}
