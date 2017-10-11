using System.Net;

namespace RDD.Domain.Exceptions
{
    public class NotFoundException : HttpLikeException
    {
        public NotFoundException()
            : base(HttpStatusCode.NotFound) { }

        public NotFoundException(string message)
            : base(HttpStatusCode.NotFound, message) { }
    }
}
