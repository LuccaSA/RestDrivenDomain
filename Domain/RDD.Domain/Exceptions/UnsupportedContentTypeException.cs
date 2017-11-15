using System;
using System.Net;

namespace RDD.Domain.Exceptions
{
    public class UnsupportedContentTypeException : BusinessException
    {
        public UnsupportedContentTypeException(string message) 
            : base(message)
        {
        }

        public UnsupportedContentTypeException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }

        public override HttpStatusCode StatusCode => HttpStatusCode.UnsupportedMediaType;
    }
}