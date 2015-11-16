using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Infra.Exceptions
{
	public class SqlUniqConstraintException : Exception
	{
		public SqlUniqConstraintException(string message)
			: base(message) { }
	}
}
