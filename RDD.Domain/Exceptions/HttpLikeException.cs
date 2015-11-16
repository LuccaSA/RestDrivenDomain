using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain.Exceptions
{
	public class HttpLikeException : Exception
	{
		public HttpStatusCode Status { get; private set; }

		public override string Message { get { return base.Message; } }

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

		public HttpLikeException(HttpStatusCode status, string message, Exception innerException)
			: base(message, innerException)
		{
			this.Status = status;
		}
		public HttpLikeException(HttpStatusCode status) : this(status, null, null) { }
		public HttpLikeException(HttpStatusCode status, string message) : this(status, message, null) { }
		public HttpLikeException(HttpStatusCode status, Exception innerException) : this(status, innerException.Message, innerException) { }
	}
}
