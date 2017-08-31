using RDD.Domain;
using RDD.Domain.Contexts;
using RDD.Infra.Contexts;
using RDD.Infra.DependencyInjection;
using RDD.Infra.Helpers;
using RDD.Infra.Logs;
using RDD.Infra.Services;

namespace RDD.Infra.BootStrappers
{
	public static class TestsBootStrapper
	{
		public static void ApplicationStart()
		{
			var resolver = new DependencyInjectionResolver();

			resolver.Register<IAsyncService>(() => new AsyncService());
			resolver.Register<IExecutionModeProvider>(() => new TestExecutionModeProvider());
			resolver.Register<ILogService>(() => new LostLogService());

			Resolver.Current = () => resolver;
		}

		public static void ApplicationBeginRequest()
		{
			var resolver = Resolver.Current();
			var webContext = new InMemoryWebContext();
			webContext.Items["executionContext"] = new HttpExecutionContext();

			resolver.Register<IWebContext>(() => webContext);
			resolver.Register<IExecutionContext>(() =>
			{
				return (IExecutionContext)resolver.Resolve<IWebContext>().Items["executionContext"];
			});
		}
	}
}
