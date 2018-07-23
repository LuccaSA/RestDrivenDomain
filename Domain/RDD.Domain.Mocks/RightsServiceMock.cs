using RDD.Domain.Models.Querying;
using RDD.Domain.Rights;
using System;
using System.Linq.Expressions;

namespace RDD.Domain.Mocks
{
    public class RightsServiceMock : IRightExpressionsHelper
    {
        public Expression<Func<T, bool>> GetFilter<T>(Query<T> query) where T : class => t => true;
    }
}