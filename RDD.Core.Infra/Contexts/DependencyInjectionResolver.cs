using RDD.Domain;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Infra.Contexts
{
	public class DependencyInjectionResolver : IDependencyInjectionResolver
	{
		private ConcurrentDictionary<Type, object> _mappings = new ConcurrentDictionary<Type, object>();

		public void Register<TInterface>(Func<TInterface> constructor)
		{
			_mappings[typeof(TInterface)] = constructor;
		}

		public TInterface Resolve<TInterface>()
		{
			var type = typeof(TInterface);

			if (!_mappings.ContainsKey(type))
			{
				throw new Exception(String.Format("Type {0} not handled by dependency injection", type));
			}

			return ((Func<TInterface>)_mappings[type])();
		}
	}
}
