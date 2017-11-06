using System;
using System.Net;

namespace RDD.Domain.Exceptions
{
    /// <summary>
    /// Should be used for functional exceptions.
    /// Will be translated as HttpStatusCode.BadRequest : 400
    /// </summary>
    public class FunctionalException : Exception, IStatusCodeException
    {
        public FunctionalException(string message) 
            : base(message)
        {
        }

        public FunctionalException(string message, Exception innerException) 
            : base(message,  innerException)
        {
        }

        public virtual HttpStatusCode StatusCode => HttpStatusCode.BadRequest;
    }
}
