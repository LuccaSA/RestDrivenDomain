using RDD.Domain;
using SimpleInjector;
using System;

namespace RDD.Infra.DependencyInjection
{
	public class SimpleInjectorResolver : IDependencyInjectionResolver
	{
		private readonly Container SimpleInjectorContainer;

		public SimpleInjectorResolver(Container container)
		{
			SimpleInjectorContainer = container;
		}

		public void Register<TInterface>(Func<TInterface> constructor) where TInterface : class
		{
			throw new NotSupportedException("Use Simple Injector container directly");
		}

		public void Register<TInterface, TArg1>(Func<TArg1, TInterface> constructor) where TInterface : class
		{
			throw new NotSupportedException("Use Simple Injector container directly");
		}

		public TInterface Resolve<TInterface>() where TInterface : class
		{
			return SimpleInjectorContainer.GetInstance<TInterface>();
		}

		public TInterface Resolve<TInterface, TArg1>(TArg1 arg1) where TInterface : class
		{
			throw new NotSupportedException("Dependency injection with Simple Injector container does not handle args injection");
		}
	}
}
