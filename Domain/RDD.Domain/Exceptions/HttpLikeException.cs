using System;
using System.Net;

namespace RDD.Domain.Exceptions
{
	public class HttpLikeException : Exception
	{
		public HttpStatusCode Status { get; private set; }

		public override string Message => base.Message;

	    public override string StackTrace
		{
			get
			{
				if (base.StackTrace == null)
				{
					if (InnerException != null)
					{
						return InnerException.StackTrace;
					}
				}
				return base.StackTrace;
			}
		}

		public HttpLikeException(HttpStatusCode status) : this(status, null, null) { }
		public HttpLikeException(HttpStatusCode status, string message) : this(status, message, null) { }
		public HttpLikeException(HttpStatusCode status, Exception innerException) : this(status, innerException.Message, innerException) { }
		public HttpLikeException(HttpStatusCode status, string message, Exception innerException)
			: base(message, innerException)
		{
			Status = status;
		}

		public static HttpLikeException Parse(Exception e)
		{
			return e as HttpLikeException ?? new HttpLikeException(HttpStatusCode.InternalServerError, e);
		}
	}
}
