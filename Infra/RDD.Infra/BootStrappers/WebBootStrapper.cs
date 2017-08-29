using Microsoft.AspNetCore.Http;
using RDD.Domain;
using RDD.Domain.Contexts;
using RDD.Infra.Contexts;
using RDD.Infra.Helpers;
using RDD.Infra.Logs;
using RDD.Infra.Services;
using System.Threading;
using HttpContextWrapper = RDD.Infra.Contexts.HttpContextWrapper;

namespace RDD.Infra.BootStrappers
{
	public static class WebBootStrapper
	{
		public static void ApplicationStart()
		{
			var resolver = new DependencyInjectionResolver();

			resolver.Register<IWebContext, HttpContext>((HttpContext context) =>
			{
				return new HttpContextWrapper(context);
			});

			resolver.Register<IWebContext>(() =>
			{
				return AsyncService.ThreadedContexts[Thread.CurrentThread.ManagedThreadId];
			});

			resolver.Register<IExecutionContext>(() =>
			{
				var webContext = resolver.Resolve<IWebContext>();

				return (IExecutionContext)webContext.Items["executionContext"];
			});
			resolver.Register<IAsyncService>(() => new AsyncService());
			resolver.Register<ILogService>(() => new LostLogService());
			resolver.Register<IExecutionModeProvider>(() => new DevExecutionModeProvider());

			Resolver.Current = () => resolver;
		}

		public static void ApplicationBeginRequest()
		{
			var webContext = Resolver.Current().Resolve<IWebContext>();
			webContext.Items["executionContext"] = new HttpExecutionContext();
		}
	}
}
