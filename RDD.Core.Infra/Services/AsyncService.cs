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
using System.Web.Hosting;

namespace RDD.Infra.Services
{
    public class AsyncService : IAsyncService
    {
        public static AsyncLocal<IWebContext> WebContextAccessor { get; } = new AsyncLocal<IWebContext>();

        public void ContinueAsync(Action action)
        {
            var items = Resolver.Current().Resolve<IWebContext>().Items;
            WebContextAccessor.Value = new InMemoryWebContext(items);

            HostingEnvironment.QueueBackgroundWorkItem(c => action());
        }

        public void RunInParallel<TEntity>(IEnumerable<TEntity> entities, Action<TEntity> action)
        {
            RunInParallel(entities, new ParallelOptions(), action, new List<string>());
        }

        public void RunInParallel<TEntity>(IEnumerable<TEntity> entities, ParallelOptions options, Action<TEntity> action)
        {
            RunInParallel(entities, options, action, new List<string>());
        }

        public void RunInParallel<TEntity>
        (
            IEnumerable<TEntity> entities,
            ParallelOptions options,
            Action<TEntity> action,
            IReadOnlyCollection<string> persistedInjectionTokens
        )
        {
            var items = Resolver.Current().Resolve<IWebContext>().Items;
            WebContextAccessor.Value = new InMemoryWebContext(items,persistedInjectionTokens);

            Parallel.ForEach<TEntity>(entities, options, (entity) =>
            {
                action(entity);
            });
        }
    }
}
