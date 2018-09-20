using System.Net;

namespace RDD.Domain.Exceptions
{
    public class ForbiddenException : BusinessException
    {
        public ForbiddenException(string message)
            : base(message)
        {
        }

        public override HttpStatusCode StatusCode => HttpStatusCode.Forbidden;
    }
}