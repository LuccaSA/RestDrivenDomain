using Rdd.Infra.Web.Models;
using System;
using System.Linq.Expressions;

namespace Rdd.Infra.Rights
{
    public interface IRightExpressionsHelper<T>
         where T : class
    {
        Expression<Func<T, bool>> GetFilter(Query<T> query);
    }
}