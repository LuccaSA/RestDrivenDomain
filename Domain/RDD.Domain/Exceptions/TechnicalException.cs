using System;
using System.Net;

namespace RDD.Domain.Exceptions
{
    /// <summary>
    /// Should be used for internal / technical exceptions only. 
    /// Will be translated as HttpStatusCode.InternalServerError : 500
    /// </summary>
    public class TechnicalException : Exception, IStatusCodeException
    {
        public TechnicalException(string message)
            : base(message)
        {
        }

        public TechnicalException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public virtual HttpStatusCode StatusCode => HttpStatusCode.InternalServerError;
    }
}