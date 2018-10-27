using Rdd.Domain.Helpers;
using Rdd.Domain.Helpers.Expressions;
using System.Collections.Generic;

namespace Rdd.Domain.Models.Querying
{
    public interface IQuery<TEntity>
        where TEntity : class
    {
        HttpVerbs Verb { get; }
        IExpressionTree<TEntity> Fields { get; }
        Filter<TEntity> Filter { get; }
        List<OrderBy<TEntity>> OrderBys { get; }
        Page Page { get; }
        Options Options { get; }
    }
}