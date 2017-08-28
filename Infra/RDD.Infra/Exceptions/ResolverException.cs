using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Infra.Exceptions
{
	public class ResolverException : Exception
	{
		public ResolverException(string message)
			: base(message) { }
	}
}
