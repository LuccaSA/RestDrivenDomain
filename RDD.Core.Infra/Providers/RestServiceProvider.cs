using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Infra.Providers
{
	public static class RestServiceProvider
	{
		private static IDictionary<Type, Func<IStorageService, IExecutionContext, IRestService>> _cache = new Dictionary<Type, Func<IStorageService, IExecutionContext, IRestService>>();

		public static void Register<IEntity, TKey>(Func<IStorageService, IExecutionContext, IRestService<IEntity, TKey>> provider)
			where IEntity : IEntityBase<TKey>
			where TKey : IEquatable<TKey>
		{
			_cache[typeof(IEntity)] = provider;
		}

		public static IRestService<IEntity, TKey> Get<IEntity, TKey>(IStorageService storage, IExecutionContext execution)
			where IEntity : IEntityBase<TKey>
			where TKey : IEquatable<TKey>
		{
			return ((Func<IStorageService, IExecutionContext, IRestService<IEntity, TKey>>)_cache[typeof(IEntity)])(storage, execution);
		}
	}
}
