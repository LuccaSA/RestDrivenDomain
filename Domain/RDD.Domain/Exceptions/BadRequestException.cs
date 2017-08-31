using System.Net;

namespace RDD.Domain.Exceptions
{
	public class BadRequestException : HttpLikeException
	{
		public BadRequestException()
			: base(HttpStatusCode.BadRequest) { }

		public BadRequestException(string message)
			: base(HttpStatusCode.BadRequest, message) { }
	}
}
