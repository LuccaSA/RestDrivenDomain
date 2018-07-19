using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying;
using RDD.Domain.Rights;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RDD.Domain.Mocks
{
    public class RightsServiceMock : IRightsService
    {
        public Expression<Func<T, bool>> GetFilter<T>(Query<T> query) where T : class => t => true;

        public HashSet<int> GetOperationIds<T>(HttpVerbs verb) => new HashSet<int>();

        public bool IsAllowed<T>(HttpVerbs verb) => true;
    }
}
