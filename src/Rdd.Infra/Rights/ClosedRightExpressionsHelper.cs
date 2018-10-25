using Rdd.Infra.Web.Models;
using System;
using System.Linq.Expressions;

namespace Rdd.Infra.Rights
{
    public class ClosedRightExpressionsHelper<T> : IRightExpressionsHelper<T>
         where T : class
    {
        public Expression<Func<T, bool>> GetFilter(Query<T> query) => t => false;
    }
}