using System;
using System.Net;

namespace RDD.Domain.Exceptions
{
    public class UnreachableEntityTypeException<TEntity> : HttpLikeException
    {
        public UnreachableEntityTypeException()
            : base(HttpStatusCode.Forbidden, String.Format("Unreachable entity type {0}. Consider adding Combinations to your Application.", typeof(TEntity).Name)) { }
    }
}
