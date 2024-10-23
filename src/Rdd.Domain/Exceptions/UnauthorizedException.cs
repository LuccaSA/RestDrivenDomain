using System;
using System.Net;

namespace Rdd.Domain.Exceptions
{
    [Serializable]
    public sealed class UnauthorizedException : BusinessException
    {
        public UnauthorizedException(string message)
            : base(message)
        {
        }

        public override HttpStatusCode StatusCode => HttpStatusCode.Unauthorized;
    }
}