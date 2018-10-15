using Rdd.Domain.Models.Querying;
using System;
using System.Linq.Expressions;

namespace Rdd.Domain.Rights
{
    public class OpenRightExpressionsHelper<T> : IRightExpressionsHelper<T>
         where T : class
    {
        public Expression<Func<T, bool>> GetFilter(Query<T> query) => t => true;
    }
}