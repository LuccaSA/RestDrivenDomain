using System.Linq;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Domain.Models.Querying;

namespace Rdd.Domain
{
    public interface IIncludeApplicator
    {
        IQueryable<TEntity> ApplyIncludes<TEntity>(IQueryable<TEntity> entities, Query<TEntity> query, IExpressionTree includeWhiteList)
            where TEntity : class;
    }
}
