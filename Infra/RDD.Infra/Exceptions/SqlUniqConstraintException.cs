using System;

namespace RDD.Infra.Exceptions
{
	public class SqlUniqConstraintException : Exception
	{
		public SqlUniqConstraintException(string message)
			: base(message) { }
	}
}
