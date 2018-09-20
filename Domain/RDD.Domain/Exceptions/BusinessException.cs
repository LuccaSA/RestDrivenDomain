using System;
using System.Net;

namespace RDD.Domain.Exceptions
{
    /// <summary>
    /// Should be used for functional exceptions.
    /// </summary>
    public abstract class BusinessException : ApplicationException, IStatusCodeException
    {
        protected BusinessException(string message) 
            : base(message)
        {
        }

        protected BusinessException(string message, Exception innerException) 
            : base(message,  innerException)
        {
        }

        public abstract HttpStatusCode StatusCode { get; }
    }
}
