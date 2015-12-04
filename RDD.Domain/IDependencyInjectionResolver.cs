using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain
{
	public interface IDependencyInjectionResolver
	{
		void Register<TInterface>(Func<TInterface> constructor);
		void Register<TInterface, TArg1>(Func<TArg1, TInterface> constructor);

		TInterface Resolve<TInterface>();
		TInterface Resolve<TInterface, TArg1>(TArg1 arg1);
	}
}
