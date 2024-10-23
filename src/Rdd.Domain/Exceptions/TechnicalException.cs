using System;
using System.Net;

namespace Rdd.Domain.Exceptions
{
    /// <summary>
    /// Should be used for internal / technical exceptions only. 
    /// Will be translated as HttpStatusCode.InternalServerError : 500
    /// </summary>
    [Serializable]
    public sealed class TechnicalException : ApplicationException, IStatusCodeException
    {
        public TechnicalException(string message)
            : base(message)
        {
        }

        public TechnicalException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public HttpStatusCode StatusCode => HttpStatusCode.InternalServerError;
    }
}