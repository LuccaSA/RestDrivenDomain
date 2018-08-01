using System;
using System.Linq.Expressions;
using RDD.Domain.Models.Querying;
using RDD.Domain.Rights;

namespace RDD.Web.Helpers
{
    /// <summary>
    /// Default right helper : everything is allowed.
    /// With AddRddRights<>() you can register specific ICombinationsHolder or IRightExpressionsHelper to implement custom rights
    /// </summary>
    public class DefaultRightExpressionsHelper<T> : IRightExpressionsHelper<T>
        where T : class
    {
        public Expression<Func<T, bool>> GetFilter(Query<T> query)
        {
            return t => true;
        }
    }
}
