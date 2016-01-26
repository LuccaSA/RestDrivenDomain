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

		public HttpLikeException(HttpStatusCode status, string message, Exception innerException, params object[] args)
			: base(String.Format(message, args), innerException)
		{
			this.Status = status;
		}
		public HttpLikeException(HttpStatusCode status) : this(status, null, null, new HashSet<object>().ToArray()) { }
		public HttpLikeException(HttpStatusCode status, string message) : this(status, message, null, new HashSet<object>().ToArray()) { }
		public HttpLikeException(HttpStatusCode status, string message, params object[] args) : this(status, message, null, args) { }
		public HttpLikeException(HttpStatusCode status, Exception innerException) : this(status, innerException.Message, innerException, new HashSet<object>().ToArray()) { }

		public static HttpLikeException Parse(Exception e)
		{
			return e as HttpLikeException ?? new HttpLikeException(HttpStatusCode.InternalServerError, e);
		}
	}
}
