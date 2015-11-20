using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RDD.Domain.Contexts
{
	public static class WebContext
	{
		public static Func<IWebContext> Current { get; set; }
		public static Func<IDictionary, IWebContext> AsyncContext { get; set; }

		//Permet de faire le lien entre un thread fils lancé en async et le web context
		public static ConcurrentDictionary<int, IWebContext> ThreadedContexts = new ConcurrentDictionary<int, IWebContext>();

		public static IWebContext NewAsyncWebContext(IDictionary items)
		{
			var context = AsyncContext(items);

			ThreadedContexts.AddOrUpdate(Thread.CurrentThread.ManagedThreadId, context, (key, value) => value);

			return context;
		}
	}
}
