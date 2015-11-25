using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain.Contexts
{
	public static class Resolver
	{
		public static Func<IDependencyInjectionResolver> Current { get; set; }
	}
}
