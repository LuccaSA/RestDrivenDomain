using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain.Contexts
{
	public static class ExecutionContext
	{
		public static IExecutionContext Current
		{
			get
			{
				return (IExecutionContext)Resolver.Current().Resolve<IWebContext>().Items["executionContext"];
			}
		}
	}
}
