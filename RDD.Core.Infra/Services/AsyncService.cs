using RDD.Domain;
using RDD.Domain.Contexts;
using RDD.Infra.Contexts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RDD.Infra.Services
{
	public class AsyncService : IAsyncService
	{
		public static ConcurrentDictionary<int, IWebContext> ThreadedContexts = new ConcurrentDictionary<int, IWebContext>();

		public void ContinueAsync(Action action)
		{
			var items = Resolver.Current().Resolve<IWebContext>().Items;
			var context = new InMemoryWebContext(items);

			Task.Factory.StartNew(() =>
			{
				AsyncService.ThreadedContexts.AddOrUpdate(Thread.CurrentThread.ManagedThreadId, context, (key, value) => value);

				action();
			});
		}
	}
}
