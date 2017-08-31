using System;

namespace RDD.Infra.Exceptions
{
	public class ResolverException : Exception
	{
		public ResolverException(string message)
			: base(message) { }
	}
}
