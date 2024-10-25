using System;
using System.Net;
using System.Runtime.Serialization;

namespace Rdd.Domain.Exceptions
{
    [Serializable]
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
