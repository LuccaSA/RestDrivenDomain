using System;
using System.Net;

namespace RDD.Domain.Exceptions
{
    public class UnauthorizedException : BusinessException
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