using RDD.Domain;
using RDD.Infra.Exceptions;
using System;
using System.Collections.Concurrent;

namespace RDD.Infra.DependencyInjection
{
	public class DependencyInjectionResolver : IDependencyInjectionResolver
	{
		private ConcurrentDictionary<Type, object> _mappings = new ConcurrentDictionary<Type, object>();
		private ConcurrentDictionary<Tuple<Type, Type>, object> _mappingsWithOneArg = new ConcurrentDictionary<Tuple<Type, Type>, object>();

		//TODO : mutualiser le code des différentes méthodes
		public void Register<TInterface>(Func<TInterface> constructor) where TInterface : class
		{
			_mappings[typeof(TInterface)] = constructor;
		}
		public void Register<TInterface, TArg1>(Func<TArg1, TInterface> constructor) where TInterface : class
		{
			_mappingsWithOneArg[new Tuple<Type, Type>(typeof(TInterface), typeof(TArg1))] = constructor;
		}

		public TInterface Resolve<TInterface>() where TInterface : class
		{
			var type = typeof(TInterface);

			if (!_mappings.ContainsKey(type))
			{
				throw new ResolverException(String.Format("Type {0} not handled by dependency injection", type));
			}

			return ((Func<TInterface>)_mappings[type])();
		}
		public TInterface Resolve<TInterface, TArg1>(TArg1 arg1) where TInterface : class
		{
			var tuple = new Tuple<Type, Type>(typeof(TInterface), typeof(TArg1));

			if (!_mappingsWithOneArg.ContainsKey(tuple))
			{
				throw new ResolverException(String.Format("Type {0} with argument {1} not handled by dependency injection", tuple.Item1, tuple.Item2));
			}

			return ((Func<TArg1, TInterface>)_mappingsWithOneArg[tuple])(arg1);
		}
	}
}
