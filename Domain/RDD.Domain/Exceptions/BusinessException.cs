using System;
using System.Net;

namespace RDD.Domain.Exceptions
{
    /// <summary>
    /// Should be used for functional exceptions.
    /// </summary>
    public abstract class BusinessException : Exception, IStatusCodeException
    {
        public BusinessException(string message) 
            : base(message)
        {
        }

        public BusinessException(string message, Exception innerException) 
            : base(message,  innerException)
        {
        }

        public abstract HttpStatusCode StatusCode { get; }
    }
}
