using System;
using System.Net;

namespace Rdd.Domain.Exceptions
{
    /// <summary>
    /// Should be used for functional exceptions.
    /// </summary>
    [Serializable]
    public abstract class BusinessException : ApplicationException, IStatusCodeException
    {
        protected BusinessException(string message)
            : base(message)
        {
        }

        protected BusinessException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public abstract HttpStatusCode StatusCode { get; }
    }
}