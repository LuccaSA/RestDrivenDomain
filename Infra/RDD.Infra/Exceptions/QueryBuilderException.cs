using Rdd.Domain.Exceptions;
using System;
using System.Net;

namespace Rdd.Infra.Exceptions
{
    public class QueryBuilderException : BusinessException
    {
        public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;

        public QueryBuilderException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}