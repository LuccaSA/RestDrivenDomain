using RDD.Domain.Models.Querying;
using System;
using System.Linq.Expressions;

namespace RDD.Domain.Rights
{
    public interface IRightExpressionsHelper<T>
         where T : class
    {
        Expression<Func<T, bool>> GetFilter(Query<T> query);
    }
}