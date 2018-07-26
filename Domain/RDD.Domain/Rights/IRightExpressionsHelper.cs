using RDD.Domain.Models.Querying;
using System;
using System.Linq.Expressions;

namespace RDD.Domain.Rights
{
    public interface IRightExpressionsHelper
    {
        Expression<Func<T, bool>> GetFilter<T>(Query<T> query) where T : class;
    }
}