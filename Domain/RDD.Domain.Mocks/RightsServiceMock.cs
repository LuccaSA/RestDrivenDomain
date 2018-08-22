using RDD.Domain.Models.Querying;
using RDD.Domain.Rights;
using System;
using System.Linq.Expressions;

namespace RDD.Domain.Mocks
{
    public class RightsServiceMock<T> : IRightExpressionsHelper<T>
        where T : class
    {
        public Expression<Func<T, bool>> GetFilter(Query<T> query) => t => true;
    }
}