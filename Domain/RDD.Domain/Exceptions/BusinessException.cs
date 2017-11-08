using System;
using System.Net;

namespace RDD.Domain.Exceptions
{
    /// <summary>
    /// Should be used for functional exceptions.
    /// Will be translated as HttpStatusCode.BadRequest : 400
    /// </summary>
    public class BusinessException : Exception, IStatusCodeException
    {
        public BusinessException(string message) 
            : base(message)
        {
        }

        public BusinessException(string message, Exception innerException) 
            : base(message,  innerException)
        {
        }

        public virtual HttpStatusCode StatusCode => HttpStatusCode.BadRequest;
    }
}
