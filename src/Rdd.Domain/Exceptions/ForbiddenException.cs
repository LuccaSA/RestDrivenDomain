using System;
using System.Net;

namespace Rdd.Domain.Exceptions
{
    [Serializable]
    public sealed class ForbiddenException : BusinessException
    {
        public ForbiddenException(string message)
            : base(message)
        {
        }

        public override HttpStatusCode StatusCode => HttpStatusCode.Forbidden;
    }
}