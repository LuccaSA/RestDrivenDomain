using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain
{
	public interface IAsyncService
	{
		void ContinueAsync(Action action);
		void RunInParallel<TEntity>(IEnumerable<TEntity> entities, Action<TEntity> action);
		void RunInParallel<TEntity>(IEnumerable<TEntity> entities, ParallelOptions options, Action<TEntity> action);
		void RunInParallel<TEntity>(IEnumerable<TEntity> entities, ParallelOptions options, Action<TEntity> action, IReadOnlyCollection<string> persistedInjectionTokens);
	}
}
