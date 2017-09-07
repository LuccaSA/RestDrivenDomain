using RDD.Domain;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RDD.Web.Contexts
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
			return Task.Factory.StartNew(() =>
			{
				ThreadedContexts.AddOrUpdate(Thread.CurrentThread.ManagedThreadId, _webContext, (key, value) => value);
				action();
			});
		}

		public void RunInParallel<TEntity>(IEnumerable<TEntity> entities, Action<TEntity> action)
		{
			RunInParallel(entities, new ParallelOptions(), action);
		}

		public void RunInParallel<TEntity>(IEnumerable<TEntity> entities, ParallelOptions options, Action<TEntity> action)
		{
			Parallel.ForEach(entities, options, (entity) =>
			{
				ThreadedContexts.AddOrUpdate(Thread.CurrentThread.ManagedThreadId, _webContext, (key, value) => value);
				action(entity);
			});
		}
	}
}
