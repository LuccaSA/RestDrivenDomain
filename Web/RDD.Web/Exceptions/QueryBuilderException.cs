using System;
using System.Net;
using RDD.Domain.Exceptions;

namespace RDD.Web.Exceptions
{
    public class QueryBuilderException : BusinessException
    {
        public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;

        public QueryBuilderException(string message)
            : base(message) { }

        public QueryBuilderException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
