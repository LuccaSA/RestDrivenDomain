using System;
using System.Net;

namespace RDD.Domain.Exceptions
{
    public class BadRequestException : BusinessException
    { 
        public BadRequestException(string message) 
            : base(message)
        {
        }

        public BadRequestException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }

        public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;
    }
}
