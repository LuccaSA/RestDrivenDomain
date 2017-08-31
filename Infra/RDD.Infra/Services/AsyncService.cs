using RDD.Domain;
using RDD.Infra.Contexts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RDD.Infra.Services
{
	public class AsyncService : IAsyncService
	{
		private IWebContext _webContext;

		public static ConcurrentDictionary<int, IWebContext> ThreadedContexts = new ConcurrentDictionary<int, IWebContext>();

		public AsyncService(IWebContext webContext)
		{
			_webContext = webContext;
		}

		public Task ContinueAsync(Action action)
		{
			var items = _webContext.Items;
			var context = new InMemoryWebContext(items);

			return Task.Factory.StartNew(() =>
			{
				AsyncService.ThreadedContexts.AddOrUpdate(Thread.CurrentThread.ManagedThreadId, context, (key, value) => value);

				action();
			});
		}

		public void RunInParallel<TEntity>(IEnumerable<TEntity> entities, Action<TEntity> action)
		{
			RunInParallel(entities, new ParallelOptions(), action);
		}

		public void RunInParallel<TEntity>(IEnumerable<TEntity> entities, ParallelOptions options, Action<TEntity> action)
		{
			var items = _webContext.Items;
			var context = new InMemoryWebContext(items);

			Parallel.ForEach<TEntity>(entities, options, (entity) =>
			{
				AsyncService.ThreadedContexts.AddOrUpdate(Thread.CurrentThread.ManagedThreadId, context, (key, value) => value);

				action(entity);
			});
		}
	}
}
