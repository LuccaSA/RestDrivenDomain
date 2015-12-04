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

		//TODO : mutualiser le code des différentes méthodes
		public void Register<TInterface>(Func<TInterface> constructor)
		{
			_mappings[typeof(TInterface)] = constructor;
		}
		public void Register<TInterface, TArg1>(Func<TArg1, TInterface> constructor)
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
		public TInterface Resolve<TInterface, TArg1>(TArg1 arg1)
		{
			var type = typeof(TInterface);

			if (!_mappings.ContainsKey(type))
			{
				throw new Exception(String.Format("Type {0} not handled by dependency injection", type));
			}

			return ((Func<TArg1, TInterface>)_mappings[type])(arg1);
		}
	}
}
