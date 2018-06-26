using System;
using System.Net;

namespace RDD.Domain.Exceptions
{
    public class ForbiddenException : BusinessException
    { 
        public ForbiddenException(string message) 
            : base(message)
        {
        }

        public ForbiddenException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }

        public override HttpStatusCode StatusCode => HttpStatusCode.Forbidden;
    }
}
