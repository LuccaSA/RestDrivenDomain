using RDD.Domain.Helpers;
using System.Collections.Generic;

namespace RDD.Domain.Rights
{
    public interface IRightsService : IRightExpressionsHelper
    {
        bool IsAllowed<T>(HttpVerbs verb);
        HashSet<int> GetOperationIds<T>(HttpVerbs verb);
    }
}