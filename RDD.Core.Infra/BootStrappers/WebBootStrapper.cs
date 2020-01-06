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
	public static class WebBootStrapper
	{
		public static void ApplicationStart()
		{
			var resolver = new DependencyInjectionResolver();

			resolver.Register<IWebContext>(() =>
			{
				IWebContext result;

				if (HttpContext.Current != null)
				{
					result = new HttpContextWrapper(HttpContext.Current);
				}
				else
				{
					result = AsyncService.WebContextAccessor.Value;
				}

				return result;
			});
			resolver.Register<IExecutionContext>(() =>
			{
				var webContext = resolver.Resolve<IWebContext>();

				return (IExecutionContext)webContext.Items["executionContext"];
			});
			resolver.Register<IAsyncService>(() => new AsyncService());

			Resolver.Current = () => resolver;
		}

		public static void ApplicationBeginRequest()
		{
			var webContext = Resolver.Current().Resolve<IWebContext>();
			webContext.Items["executionContext"] = new HttpExecutionContext();
		}
	}
}
