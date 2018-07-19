using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RDD.Domain.Rights
{
    public interface IRightsService
    {
        bool IsAllowed<T>(HttpVerbs verb);
        Expression<Func<T, bool>> GetFilter<T>(Query<T> query) where T : class;
        HashSet<int> GetOperationIds<T>(HttpVerbs verb);
    }
}