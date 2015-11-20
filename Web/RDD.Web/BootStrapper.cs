using RDD.Domain.Contexts;
using RDD.Infra.Contexts;
using RDD.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using HttpContextWrapper = RDD.Web.Helpers.HttpContextWrapper;

namespace RDD.Web
{
	public static class BootStrapper
	{
		public static void ApplicationStart()
		{
			WebContext.Current = () =>
			{
				if (HttpContext.Current != null)
				{
					return new HttpContextWrapper(HttpContext.Current);
				}
				else
				{
					return WebContext.ThreadedContexts[Thread.CurrentThread.ManagedThreadId];
				}
			};
			WebContext.AsyncContext = (items) => new InMemoryWebContext(items);
		}

		public static void ApplicationBeginRequest()
		{
			var webContext = WebContext.Current();

			webContext.Items["executionContext"] = new HttpExecutionContext();
		}
	}
}
