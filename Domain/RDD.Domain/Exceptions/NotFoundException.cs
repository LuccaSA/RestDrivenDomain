using System;
using System.Net;

namespace RDD.Domain.Exceptions
{
    public class NotFoundException : BusinessException
    { 
        public NotFoundException(string message) 
            : base(message)
        {
        }

        public NotFoundException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }

        public override HttpStatusCode StatusCode => HttpStatusCode.NotFound;
    }
}
