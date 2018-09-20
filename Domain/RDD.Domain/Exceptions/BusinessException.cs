using System;
using System.Net;
using System.Runtime.Serialization;

namespace RDD.Domain.Exceptions
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

        protected BusinessException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public abstract HttpStatusCode StatusCode { get; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(StatusCode), StatusCode);
        }
    }
}