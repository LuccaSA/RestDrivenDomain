using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain.Exceptions
{
	public class UnauthorizedException : HttpLikeException
	{
		public UnauthorizedException() : this(null, null) { }
		public UnauthorizedException(string message) : this(message, null) { }
		public UnauthorizedException(Exception innerException) : this(innerException.Message, innerException) { }
		public UnauthorizedException(string message, Exception innerException) : this(message, LogLevel.INFO, innerException) { }
		public UnauthorizedException(string message, LogLevel logLevel, Exception innerException)
			: base(HttpStatusCode.Unauthorized, LogLevel.INFO, message, innerException) { }

	}
}