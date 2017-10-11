using System.Net;

namespace RDD.Domain.Exceptions
{
    public class OutOfRangeException : HttpLikeException
    {
        public OutOfRangeException()
            : base(HttpStatusCode.BadRequest) { }

        public OutOfRangeException(string message)
            : base(HttpStatusCode.BadRequest, message) { }
    }
}
