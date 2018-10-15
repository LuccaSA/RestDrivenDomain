using Rdd.Domain.Models.Querying;
using System;
using System.Linq.Expressions;

namespace Rdd.Domain.Rights
{
    public class ClosedRightExpressionsHelper<T> : IRightExpressionsHelper<T>
         where T : class
    {
        public Expression<Func<T, bool>> GetFilter(Query<T> query) => t => false;
    }
}