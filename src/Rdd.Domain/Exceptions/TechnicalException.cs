﻿using System;
using System.Net;
using System.Runtime.Serialization;

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

        private TechnicalException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public HttpStatusCode StatusCode => HttpStatusCode.InternalServerError;

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(StatusCode), StatusCode);
        }
    }
}