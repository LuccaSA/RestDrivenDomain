using Rdd.Domain.Models.Querying;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Rdd.Domain.Rights
{
    public interface IRightExpressionsHelper<T>
         where T : class
    {
        Task<Expression<Func<T, bool>>> GetFilterAsync(Query<T> query);
    }
}