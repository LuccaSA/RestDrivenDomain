using Rdd.Domain.Models.Querying;
using Rdd.Domain.Rights;
using System;
using System.Linq.Expressions;

namespace Rdd.Domain.Mocks
{
    public class RightsServiceMock<T> : IRightExpressionsHelper<T>
        where T : class
    {
        public Expression<Func<T, bool>> GetFilter(Query<T> query) => t => true;
    }
}