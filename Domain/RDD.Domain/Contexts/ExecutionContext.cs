using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain.Contexts
{
	public static class ExecutionContext
	{
		[Obsolete("You should resolve the IExecutionContext through Resolver.Current().Resolve<IExecutionContext>(), your code will then be DI ready !")]
		public static IExecutionContext Current
		{
			get
			{
				return (IExecutionContext)Resolver.Current().Resolve<IWebContext>().Items["executionContext"];
			}
		}
	}
}
