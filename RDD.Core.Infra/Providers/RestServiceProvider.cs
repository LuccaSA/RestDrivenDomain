using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Infra.Providers
{
	public static class RestServiceProvider
	{
		private static IDictionary<Type, Func<IStorageService, IExecutionContext, string, IRestService>> _cache = new Dictionary<Type, Func<IStorageService, IExecutionContext, string, IRestService>>();

		public static void Register<TEntity, TKey>(Func<IStorageService, IExecutionContext, string, IRestService<TEntity, TKey>> provider)
			where TEntity : IEntityBase<TKey>
			where TKey : IEquatable<TKey>
		{
			_cache[typeof(TEntity)] = provider;
		}

		public static IRestService<IEntity, TKey> Get<IEntity, TKey>(IStorageService storage, IExecutionContext execution, string appTag = "")
			where IEntity : IEntityBase<TKey>
			where TKey : IEquatable<TKey>
		{
			return ((Func<IStorageService, IExecutionContext, string, IRestService<IEntity, TKey>>)_cache[typeof(IEntity)])(storage, execution, appTag);
		}

		public static IRestService TryGetRepository(Type entityType, Type keyType, IStorageService storage, IExecutionContext execution, string appTag = "")
		{
			if (_cache.ContainsKey(entityType))
			{
				return (IRestService)_cache[entityType](storage, execution, appTag);
			}
			var type = typeof(IRestService<,>).MakeGenericType(entityType, keyType);
			return (IRestService)type.GetConstructor(new[] { typeof(IStorageService), typeof(IExecutionContext), typeof(string) }).Invoke(new object[] { storage, execution, appTag });
		}

	}
}
