using System;
using System.Net;

namespace Rdd.Domain.Exceptions
{
    [Serializable]
    public sealed class UnsupportedContentTypeException : BusinessException
    {
        public UnsupportedContentTypeException(string message)
            : base(message)
        {
        }

        public override HttpStatusCode StatusCode => HttpStatusCode.UnsupportedMediaType;
    }
}