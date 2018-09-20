using System.Net;

namespace RDD.Domain.Exceptions
{
    public class UnauthorizedException : BusinessException
    {
        public UnauthorizedException(string message)
            : base(message)
        {
        }

        public override HttpStatusCode StatusCode => HttpStatusCode.Unauthorized;
    }
}