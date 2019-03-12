using Rdd.Domain.Models.Querying;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Rdd.Domain.Rights
{
    public class OpenRightExpressionsHelper<T> : IRightExpressionsHelper<T>
         where T : class
    {
        public Task<Expression<Func<T, bool>>> GetFilterAsync(Query<T> query) => Task.FromResult<Expression<Func<T, bool>>>(t => true);
    }
}