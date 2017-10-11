using System;
using System.Net;

namespace RDD.Domain.Exceptions
{
    public class UnauthorizedException : HttpLikeException
    {
        public UnauthorizedException() : this(null, null) { }
        public UnauthorizedException(string message) : this(message, null) { }
        public UnauthorizedException(Exception innerException) : this(innerException.Message, innerException) { }
        public UnauthorizedException(string message, Exception innerException)
            : base(HttpStatusCode.Unauthorized, message, innerException) { }

    }
}