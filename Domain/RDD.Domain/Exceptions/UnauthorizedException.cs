using System;
using System.Net;

namespace RDD.Domain.Exceptions
{
    public class UnauthorizedException : FunctionalException
    {
        public UnauthorizedException(string message) : base(message)
        {
        }

        public UnauthorizedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public override HttpStatusCode StatusCode => HttpStatusCode.Unauthorized;
    }
}