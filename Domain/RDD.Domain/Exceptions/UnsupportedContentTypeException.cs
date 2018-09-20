using System.Net;

namespace RDD.Domain.Exceptions
{
    public class UnsupportedContentTypeException : BusinessException
    {
        public UnsupportedContentTypeException(string message)
            : base(message)
        {
        }

        public override HttpStatusCode StatusCode => HttpStatusCode.UnsupportedMediaType;
    }
}