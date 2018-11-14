using Rdd.Domain.Exceptions;
using System;
using System.Net;
using System.Runtime.Serialization;

namespace Rdd.Infra.Exceptions
{
    [Serializable]
    public sealed class QueryBuilderException : BusinessException
    {
        public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;

        public QueryBuilderException(string message) : base(message) { }
        public QueryBuilderException(string message, Exception innerException) : base(message, innerException) { }

        private QueryBuilderException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}