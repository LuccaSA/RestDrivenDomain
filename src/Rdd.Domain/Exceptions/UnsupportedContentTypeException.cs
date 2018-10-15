using System;
using System.Net;
using System.Runtime.Serialization;

namespace Rdd.Domain.Exceptions
{
    [Serializable]
    public sealed class UnsupportedContentTypeException : BusinessException
    {
        public UnsupportedContentTypeException(string message)
            : base(message)
        {
        }

        private UnsupportedContentTypeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override HttpStatusCode StatusCode => HttpStatusCode.UnsupportedMediaType;
    }
}