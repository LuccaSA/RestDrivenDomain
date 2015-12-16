using RDD.Domain;
using RDD.Domain.Contexts;
using RDD.Infra.Contexts;
using RDD.Infra.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using HttpContextWrapper = RDD.Infra.Contexts.HttpContextWrapper;

namespace RDD.Infra.BootStrappers
{
	public static class TestsBootStrapper
	{
		public static void ApplicationStart()
		{
			var resolver = new DependencyInjectionResolver();

			resolver.Register<IAsyncService>(() => new AsyncService());

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
