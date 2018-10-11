using System;
using System.Net;
using System.Runtime.Serialization;

namespace Rdd.Domain.Exceptions
{
    [Serializable]
    public sealed class ForbiddenException : BusinessException
    {
        public ForbiddenException(string message)
            : base(message)
        {
        }

        private ForbiddenException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override HttpStatusCode StatusCode => HttpStatusCode.Forbidden;
    }
}