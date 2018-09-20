using System;
using System.Net;
using System.Runtime.Serialization;

namespace RDD.Domain.Exceptions
{
    [Serializable]
    public sealed class UnauthorizedException : BusinessException
    {
        public UnauthorizedException(string message)
            : base(message)
        {
        }

        private UnauthorizedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override HttpStatusCode StatusCode => HttpStatusCode.Unauthorized;
    }
}